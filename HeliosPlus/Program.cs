using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeliosPlus.InterProcess;
using HeliosPlus.Resources;
using HeliosPlus.GameLibraries;
using HeliosPlus.Shared;
using HeliosPlus.UIForms;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Drawing;

namespace HeliosPlus {
    public enum SupportedProgramMode
    {
        RunShortcut,
        EditProfile,
        StartUpNormally
    }

    public enum SupportedGameLibrary
    {
        Unknown,
        Steam,
        Uplay
    }

    internal static class Program
    {

        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeliosPlus");
        public static string AppIconPath = Path.Combine(Program.AppDataPath, $"Icons");
        public static string AppHeliosPlusIconFilename = Path.Combine(AppIconPath, @"HeliosPlus.ico");
        public static string AppOriginIconFilename = Path.Combine(AppIconPath, @"Origin.ico");
        public static string AppSteamIconFilename = Path.Combine(AppIconPath, @"Steam.ico");
        public static string AppUplayIconFilename = Path.Combine(AppIconPath, @"Steam.ico");
        public static string AppEpicIconFilename = Path.Combine(AppIconPath, @"Epic.ico");
        //internal static string ShortcutIconCachePath;


        
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {

            // Write the Application Name
            Console.WriteLine($"{Application.ProductName} v{Application.ProductVersion}");
            for (int i = 0; i <= Application.ProductName.Length + Application.ProductVersion .Length; i++)
            {
                Console.Write("=");
            }
            Console.WriteLine("=");
            Console.WriteLine($"Copyright � Terry MacDonald 2020-{DateTime.Today.Year}");
            Console.WriteLine(@"Based on Helios Display Management - Copyright � Soroush Falahati 2017-2020");

            var app = new CommandLineApplication();

            //app.Name = "HeliosDM+";
            //app.Name = Assembly.GetEntryAssembly().GetName().Name;
            app.Description = "This application helps configure your NVIDIA Videocard for multiple displays.";
            app.ExtendedHelpText = "This application helps configure your NVIDIA Videocard for multiple displays. It has some nifty features such as the "
                + Environment.NewLine + " ability to temporarily change your screen settings while you are playing a game, and then change them back once finished.";

            app.GetFullNameAndVersion();
            app.MakeSuggestionsInErrorMessage = true;
            app.HelpOption("-?|-h|--help", inherited:true);

            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            });

            // This is the RunShortcut command
            app.Command(SupportedProgramMode.RunShortcut.ToString(), (switchProfileCmd) =>
            {
                var argumentShortcut = switchProfileCmd.Argument("\"SHORTCUT_UUID\"", "(required) The UUID of the shortcut to run from those stored in the shortcut library.").IsRequired();
                argumentShortcut.Validators.Add(new ShortcutMustExistValidator());

                //description and help text of the command.
                switchProfileCmd.Description = "Use this command to run favourite game or application with a display profile of your choosing.";

                switchProfileCmd.OnExecute(() =>
                {
                    // 
                    RunShortcut(argumentShortcut.Value);
                    return 0;
                });
            });


            /*// This is the EditProfile command
            app.Command(SupportedProgramMode.EditProfile.ToString(), (editProfileCmd) =>
            {
                //description and help text of the command.
                editProfileCmd.Description = "Use this command to edit a HeliosDMPlus profile.";

                var optionProfile = editProfileCmd.Option("-p|--profile", "The Profile Name or Profile ID of the profile to you want to use.", CommandOptionType.SingleValue).IsRequired();
                optionProfile.Validators.Add(new ProfileMustExistValidator());
                
                editProfileCmd.OnExecute(() =>
                {
                    Console.WriteLine($"Editing profile {optionProfile.Value()}");

                    EditProfile(
                        GetProfile(optionProfile.Value())
                    );

                    return 0;
                });

            });*/

            app.OnExecute(() =>
            {

                Console.WriteLine("Starting Normally...");
                StartUpNormally();
                return 0;
            });


            try
            {
                // This begins the actual execution of the application
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/Main exception: {ex.Message}: {ex.InnerException}");
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
            }
            return 0;
            //return app.Execute(args);
        }

        private static void StartUpNormally()
        {
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                // Start the IPC Service to 
                if (!IPCService.StartService())
                {
                    throw new Exception(Language.Can_not_open_a_named_pipe_for_Inter_process_communication);
                }


                // Create the Shortcut Icon Cache if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppIconPath))
                {
                    try
                    {
                        Directory.CreateDirectory(AppIconPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Program/StartUpNormally exception: {ex.Message}: {ex.InnerException}");
                        // TODO
                    }
                }

                try
                {
                    // Save a copy of the HeliosPlus Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppHeliosPlusIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.HeliosPlus;
                        using (FileStream fs = new FileStream(AppHeliosPlusIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                    // Save a copy of the Steam Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppSteamIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Steam;
                        using (FileStream fs = new FileStream(AppSteamIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                    // Save a copy of the Uplay Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppUplayIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Uplay;
                        using (FileStream fs = new FileStream(AppUplayIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                    // Save a copy of the Epic Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppEpicIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Epic;
                        using (FileStream fs = new FileStream(AppEpicIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                    // Save a copy of the Origin Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppOriginIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Origin;
                        using (FileStream fs = new FileStream(AppOriginIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Program/StartUpNormally exception 2: {ex.Message}: {ex.InnerException}");
                    // TODO
                }

                IPCService.GetInstance().Status = InstanceStatus.User;
                Application.Run(new UIForms.MainForm());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/StartUpNormally exception 3: {ex.Message}: {ex.InnerException}");
                MessageBox.Show(
                    ex.Message,
                    Language.Fatal_Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            
        }


        // ReSharper disable once CyclomaticComplexity
        private static void RunShortcut(string shortcutUUID)
        {
            ShortcutItem shortcutToRun = null;

            // Check there is only one version of this application so we won't
            // mess with another monitoring session
            if (
                IPCClient.QueryAll()
                    .Any(
                        client =>
                            client.Status == InstanceStatus.Busy ||
                            client.Status == InstanceStatus.OnHold))
            {
                throw new Exception(
                    Language
                        .Another_instance_of_this_program_is_in_working_state_Please_close_other_instances_before_trying_to_switch_profile);
            }

            // Match the ShortcutName to the actual shortcut listed in the shortcut library
            // And error if we can't find it.
            if (ShortcutRepository.ContainsShortcut(shortcutUUID))
            {
                // make sure we trim the "" if there are any
                shortcutUUID = shortcutUUID.Trim('"');
                shortcutToRun = ShortcutRepository.GetShortcut(shortcutUUID);
            }
            else
            {
                throw new Exception(Language.Cannot_find_shortcut_in_library);
            }

            if (shortcutToRun is ShortcutItem)
            {
                ShortcutRepository.RunShortcut(shortcutToRun);
            }

            IPCService.GetInstance().Status = InstanceStatus.Busy;

        }

       /* private static void EditProfile(ProfileItem profile)
        {
            // Get the status of the thing
            IPCService.GetInstance().Status = InstanceStatus.User;
            // Load all the profiles from JSON
            //ProfileRepository.AllProfiles
            // Start up the DisplayProfileForm directly
            new DisplayProfileForm(profile).ShowDialog();
            // Then we close down as we're only here to edit one profile
            Application.Exit();
        }
*/
        public static bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

/*        public static string GetValidFilename(string uncheckedFilename)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                uncheckedFilename = uncheckedFilename.Replace(c.ToString(), "");
            }
            return uncheckedFilename;
        }*/
    }
}
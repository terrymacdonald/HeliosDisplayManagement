﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagician.GameLibraries
{
    public class GameLibrary
    {

        public enum SupportedGameLibraryType
        {
            Unknown,
            Origin,
            Steam,
            Ubiconnect
        }

        public struct GameAppInfo
        {
            public string GameID;
            public string GameName;
            public string GameExePath;
            public string GameInstallDir;
            public string GameIconPath;
        }

        #region Class Properties
        public virtual List<Game> AllInstalledGames { get; set; }

        public virtual int InstalledGameCount { get; set; }

        public virtual string GameLibraryName { get; set; }

        public virtual SupportedGameLibraryType GameLibraryType { get; set; }
        
        public virtual string GameLibraryExe { get; set; }

        public virtual string GameLibraryPath { get; set; }

        public virtual bool IsGameLibraryInstalled { get; set; }
        #endregion

        #region Class Methods
        public virtual bool AddGame(Game game)
        {
            return false;
        }

        public virtual bool RemoveGame(Game game)
        {
            return false;
        }

        public virtual bool RemoveGameById(string gameId)
        {
            return false;
        }

        public virtual bool RemoveGame(string gameNameOrId)
        {
            return false;
        }

        public virtual bool ContainsGame(Game game)
        {
            return false;
        }

        public virtual bool ContainsGameById(string gameId)
        {
            return false;
        }

        public virtual bool ContainsGame(string gameNameOrId)
        {
            return false;
        }


        public virtual Game GetGame(string gameNameOrId)
        {
            return null;
        }

        public virtual Game GetGameById(string gameId)
        {
            return null;
        }

        public virtual bool LoadInstalledGames()
        {           
            return false;
        }

        #endregion

    }

    [global::System.Serializable]
    public class GameLibraryException : Exception
    {
        public GameLibraryException() { }
        public GameLibraryException(string message) : base(message) { }
        public GameLibraryException(string message, Exception inner) : base(message, inner) { }
        protected GameLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}

namespace Gallery.Core.Exceptions {
    public class MissingConnectionStringSettingsException : MissingAppSettingException {
        public MissingConnectionStringSettingsException(string key)
            : base(string.Format("connection string setting '{0}'", key))
        { }
    }
}
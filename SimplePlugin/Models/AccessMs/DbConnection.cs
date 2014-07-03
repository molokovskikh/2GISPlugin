using System;

namespace SimplePlugin.Models.AccessMs
{
    /// <summary>
    /// Соединение с БД MS Access
    /// </summary>
    public class DbConnection:System.Data.IDbConnection
    {
        static readonly System.Data.OleDb.OleDbConnection _dbconn = new System.Data.OleDb.OleDbConnection();
        static string _connStr = null;

        static void _initConfString()
        {
            if(string.IsNullOrEmpty(_dbconn.ConnectionString))
               _dbconn.ConnectionString = _connStr;
        }

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static DbConnection()
        {
            //Поиск строки подключения c в файлах конфигурации c providerName="MSAccess"
            //и дальнейшая инициализация соединения
            /*
             <?xml version="1.0" encoding="utf-8"?>
             <configuration>
             <connectionStrings>
                <add name="Access" connectionString="Provider=Microsoft.Jet.OLEDB.4.0;Data Source=[PathToMyStore]\Realty.mdb" providerName="MSAccess"/>
             </connectionStrings>
             </configuration>
             */

            //Список возможных имен провайдера MSAccess
            string[] namesProviderMSAccess = new string[] { "MSAccess", "AccessMS", "Access" };
           
            System.Configuration.Configuration _conf = null;
            //Читаем базовый файл конфигурации, т.е. grym.exe.config
            try
            {
                _conf = System.Configuration.ConfigurationManager.ConnectionStrings.CurrentConfiguration;
            }
            catch (System.Configuration.ConfigurationErrorsException config_exception)
            {
                System.Windows.Forms.MessageBox.Show(config_exception.ToString(), "Ошибка чтения файла конфигурации");
            }

            //Проверка двух файлов конфигурации
            for (int c = 2; c > 0; )
            {
                //Первым проверим базовый файл конфигурации, т.е. grym.exe.config
                if (_conf != null && _conf.ConnectionStrings!=null)
                {                    
                    for (int i = 0; i < _conf.ConnectionStrings.ConnectionStrings.Count; i++)
                    {                        
                        foreach (string _cs in namesProviderMSAccess)
                            if (_cs.Equals(_conf.ConnectionStrings.ConnectionStrings[i].ProviderName))
                            {
                                _connStr = _conf.ConnectionStrings.ConnectionStrings[i].ConnectionString;
                                _dbconn.ConnectionString = _connStr;
                                //System.Windows.Forms.MessageBox.Show(_dbconn.ConnectionString);
                                return; //Если строка соединения с БД найдена то выходим
                            }
                    }
                }

                _conf = null;
                //Получим путь до сборки с плагином 
                //и поищем в каталоге где находится сборка, одноименный файл SimplePlugin.config
                string assembly_fullPath = typeof(DbConnection).Assembly.Location;
                string cfg_file = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly_fullPath),System.IO.Path.GetFileNameWithoutExtension(assembly_fullPath)+".config");
                cfg_file = !System.IO.File.Exists(cfg_file)?assembly_fullPath+".config":cfg_file;
                
                if (System.IO.File.Exists(cfg_file))
                {                   
                    try
                    {
                        _conf = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(
                                        new System.Configuration.ExeConfigurationFileMap
                                        {
                                            ExeConfigFilename = cfg_file
                                        }
                                        , System.Configuration.ConfigurationUserLevel.None);
                    }
                    catch (System.Configuration.ConfigurationErrorsException config_exception)
                    {
                        System.Windows.Forms.MessageBox.Show(config_exception.ToString(), "Ошибка чтения файла конфигурации");
                    }                    
                }
                c--;
            }            
        }

        /// <summary>
        /// Получить открытое соединение к БД
        /// </summary>
        public static System.Data.IDbConnection Instance
        {
            get
            {
                System.Data.IDbConnection _c = new DbConnection();
                if (_c.State != System.Data.ConnectionState.Open)
                {
                    _initConfString();
                    _c.Open();
                }
                return _c;
                /*
                if (_dbconn.State != System.Data.ConnectionState.Open)
                {
                    _initConfString();
                    _dbconn.Open();
                }                
                return  _dbconn;
                */
            }
        }

        //Реализация интерфейса уровня подключения IDConnection
        #region System.Data.IDbConnection
        public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel il)
        {
           return _dbconn.BeginTransaction(il);
        }

        public System.Data.IDbTransaction BeginTransaction()
        {
            return _dbconn.BeginTransaction();
        }

        public void ChangeDatabase(string databaseName)
        {
            _dbconn.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            _dbconn.Close();
        }

        public string ConnectionString
        {
            get
            {
                _initConfString();
                return _dbconn.ConnectionString;
            }
            set
            {
                _dbconn.ConnectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get 
            {
                return _dbconn.ConnectionTimeout;
            }
        }

        public System.Data.IDbCommand CreateCommand()
        {
            return _dbconn.CreateCommand();
        }

        public string Database
        {
            get 
            {
                return _dbconn.Database;
            }
        }

        public void Open()
        {
            _initConfString();
            try
            {
                _dbconn.Open();
            }
            catch
                (Exception exc)
            {
                throw new Exception(exc.Message);
            }
        }

        public System.Data.ConnectionState State
        {
            get 
            {
                return _dbconn.State; 
            }
        }

        public void Dispose()
        {
            _dbconn.Dispose();
        }
        #endregion
    }
}

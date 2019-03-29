﻿using NUnit.Framework;
using QuickLogger.NetStandard;
using QuickLogger.NetStandard.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QuickLogger.Tests.Integration
{
    [TestFixture(Category = "Integration")]
    public class QuickLogger_Integration_Should
    {
        private static ILoggerConfigManager _configManager;
        private static ILoggerProviderProps _providerProps;
        private static ILogger _logger;
        private static string _configPath = String.Empty;
        private static string _fileloggerName = "testfilelog.log";
        private static string _fileloggerPath = "";
        private static string _configName = "qloggerconfig.json";
        private static string _environmentName = "Test Env";

        private string _lastcriticalerror;
        private string _lasterror;
        private string _lastfailtolog;

        void LoggerOnFailTolog()
        {
            _lastfailtolog += " Failed.";
        }
        private void LoggerCallbackHandler(ILoggerProvider loggerProvider)
        {
            loggerProvider.CriticalError += (x =>
                _lastcriticalerror = x);
            loggerProvider.Error += (x =>
                _lasterror = x);
            loggerProvider.FailToLog += LoggerOnFailTolog;
        }

        static string _smtpconfig = "{" +
                                    "\"Host\": \"mail.domain.com\"," +
                                    "\"UserName\": \"email@domain.com\"," +
                                    "\"Password\": \"jfs93as\"," +
                                    "\"UseSSL\": false" +
                                    "}";
        static string _mailconfig = "{" +
                                    "\"SenderName\": \"Quick.Logger Alert\"," +
                                    "\"From\": \"email@domain.com\"," +
                                    "\"Recipient\": \"alert@domain.com\"," +
                                    "\"Subject\": \"\"," +
                                    "\"Body\": \"\"," +
                                    "\"CC\": \"myemail@domain.com\"," +
                                    "\"BCC\": \"\"" +
                                    "}";

        // Change to adapt with each environment
        static Dictionary<string, object> _consoleProviderInfo = new Dictionary<string, object>()
        {
            { "LogLevel", LoggerEventTypes.LOG_ALL},
            { "ShowTimeStamp", true }, { "ShowEventColors", true }
        };
        static Dictionary<string, object> _fileProviderInfo = new Dictionary<string, object>()
        {
            { "LogLevel", LoggerEventTypes.LOG_ALL}, { "AutoFileNameByProcess", false },
            { "DailyRotate", false }, { "ShowTimeStamp", true }, { "Enabled", false }
        };
        static Dictionary<string, object> _smtpProviderInfo = new Dictionary<string, object>()
        {
            { "LogLevel", LoggerEventTypes.LOG_ALL}, { "UnderlineHeaderEventType", true },
            { "DailyRotate", false }, { "ShowTimeStamp", true }, { "ShowEventColors", true }
        };
        static Dictionary<string, object> _eventsProviderInfo = new Dictionary<string, object>()
        {
            { "LogLevel", LoggerEventTypes.LOG_ALL}, { "UnderlineHeaderEventType", true } ,
            { "DailyRotate", false }, { "ShowTimeStamp", true }, { "ShowEventColors", true }
        };
        static Dictionary<string, object> _redisProviderInfo = new Dictionary<string, object>()
        {
            { "LogLevel", LoggerEventTypes.LOG_ALL}, {"Host", "192.168.1.133" }, {"Port", "6379" },
            { "OutputAsJson", true }, { "Enabled", true }
        };

        private void DeleteFile(string fileName)
        {
            if (File.Exists(fileName)) { File.Delete(fileName); };
        }
        private bool FindStringInsideFile(string fileName, string line)
        {
            using (StreamReader sr = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
            {
                while (!sr.EndOfStream)
                {
                    if (sr.ReadLine().Contains(line)) { return true; }
                }
                return false;
            }        
        }

        [OneTimeSetUp]
        public void SetUp()
        {            
            _configPath = Directory.GetParent(Assembly.GetAssembly(typeof(QuickLogger_Integration_Should)).Location).Parent.Parent.FullName;
            _fileloggerPath = Path.Combine(_configPath, _fileloggerName);
            _configPath = Path.Combine(_configPath, _configName);            
            _configManager = new QuickLoggerFileConfigManager(_configPath);
            _logger = new QuickLoggerNative(".\\");
        }

        [TearDown]
        public void TearDown()
        {
            // Manual Free of native resources
            ((IDisposable)_logger).Dispose();

            if (File.Exists(_configPath)) { File.Delete(_configPath); }
            if (File.Exists(_fileloggerPath)) { File.Delete(_fileloggerPath); }
        }

        [Test]
        public void Add_Logger_Default_Console_Provider_To_New_Logger()
        {
            
            ILoggerProviderProps providerProps = new QuickLoggerProviderProps("Test Console Provider First test", "ConsoleProvider");
            providerProps.SetProviderInfo(_consoleProviderInfo);
            ILoggerProvider loggerProvider = new QuickLoggerProvider(providerProps);
            LoggerCallbackHandler(loggerProvider);
            _logger.AddProvider(loggerProvider);            
            _logger.Info("Works");
            _logger.Custom("Works");
            _logger.Error("Works");
            _logger.Success("Works");
            //Assert that words are shown on the console 
            _logger.RemoveProvider(loggerProvider);
        }
        [Test]
        public void Add_Logger_Default_Console_Provider_To_New_Logger_Write_5_Lines_And_DisableIT()
        {
            ILoggerProviderProps providerProps = new QuickLoggerProviderProps("Test Console Provider Second test", "ConsoleProvider");
            providerProps.SetProviderInfo(_consoleProviderInfo);
            ILoggerProvider loggerProvider = new QuickLoggerProvider(providerProps);
            _logger.AddProvider(loggerProvider);
            var currentstatus = "";
            loggerProvider.StatusChanged += (x => currentstatus = x); 
            
            for (var x = 0; x < 5; x++)
            {
                _logger.Info("Works");
                _logger.Custom("Works");
                _logger.Error("Works");
                _logger.Success("Works");
            }
            //Assert that words are shown on the console 
            _logger.DisableProvider(loggerProvider);            
            for (var x = 0; x < 5; x++)
            {
                _logger.Info("Works");
                _logger.Custom("Works");
                _logger.Error("Works");
                _logger.Success("Works");
            }
            //Assert that words are not shown on the console 
            _logger.RemoveProvider(loggerProvider);
        }

        [Test]
        public void Add_Logger_Default_File_Provider_To_New_Logger()
        {
            ILoggerProviderProps providerProps = new QuickLoggerProviderProps("Test File Provider First test", "FileProvider");
            _fileProviderInfo.Add("FileName", _fileloggerPath);
            providerProps.SetProviderInfo(_fileProviderInfo);            
            ILoggerProvider loggerProvider = new QuickLoggerProvider(providerProps);
            _logger.AddProvider(loggerProvider);            
            _logger.Info("Info line");
            _logger.Custom("Custom line");
            _logger.Error("Error line");
            _logger.Success("Success line");            
            Assert.That(FindStringInsideFile(_fileloggerPath, "Info line"), Is.True);
            Assert.That(FindStringInsideFile(_fileloggerPath, "Custom line"), Is.True);
            Assert.That(FindStringInsideFile(_fileloggerPath, "Error line"), Is.True);
            Assert.That(FindStringInsideFile(_fileloggerPath, "Success line"), Is.True);
            _logger.RemoveProvider(loggerProvider);
        }

        [Test]
        public void Init_Logger_And_Get_Logger_Version()
        {        
            var loggerversion = _logger.GetLoggerNameAndVersion();
            Assert.That(loggerversion != "", Is.True);            
        }

        [Test]
        public void Add_Logger_Default_SMTP_Provider_To_New_Logger()
        {
            ILoggerProviderProps providerProps = new QuickLoggerProviderProps("Test SMTP Provider First test", "SMTPProvider");
            providerProps.SetProviderInfo(_smtpProviderInfo);
            ILoggerProvider loggerProvider = new QuickLoggerProvider(providerProps);
            _logger.AddProvider(loggerProvider);
            _logger.Info("Info line");
            _logger.Custom("Custom line");
            _logger.Error("Error line");
            _logger.Success("Success line");
            //Assert that words are received by name 
            _logger.RemoveProvider(loggerProvider);
        }
        [Test]
        public void Add_Logger_Default_Redis_Provider_To_New_Logger()
        {
            ILoggerProviderProps providerProps = new QuickLoggerProviderProps("Test Redis (ELK) Provide First test", "RedisProvider");
            providerProps.SetProviderInfo(_redisProviderInfo);
            ILoggerProvider loggerProvider = new QuickLoggerProvider(providerProps);
            _logger.AddProvider(loggerProvider);
            _logger.Info("Info line");
            _logger.Custom("Custom line");
            _logger.Error("Error line");
            _logger.Success("Succes line");
            //Assert that words are received by name 
            _logger.RemoveProvider(loggerProvider);
        }

        [Test]
        public void Add_An_Invalid_Redis_Provider_To_New_Logger_Should_Fail()
        {
            ILoggerProviderProps providerProps = new QuickLoggerProviderProps("Test Redis (ELK) Provider Second test", "RedisProvider");
            providerProps.SetProviderInfo(_fileProviderInfo);
            ILoggerProvider loggerProvider = new QuickLoggerProvider(providerProps);
            LoggerCallbackHandler(loggerProvider);
            _logger.AddProvider(loggerProvider);
            _logger.Info("Info line");
            _logger.Custom("Custom line");
            _logger.Error("Error line");
            _logger.Success("Succes line");
            System.Threading.Thread.Sleep(5000);
            //Assert that words are received by name 
            Assert.That(string.IsNullOrEmpty(_lastfailtolog), Is.False);
            _logger.RemoveProvider(loggerProvider);
        }
    }

}
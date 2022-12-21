using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Tenaris.Library.ConnectionMonitor;
using Tenaris.Library.Framework.Factory;
using Tenaris.Library.Log;
using Tenaris.Manager.Forum.Shared;
using Tenaris.Manager.Mail.Shared;
using Tenaris.Tamsa.LockVerification.Manager.Shared;
using System.Security.Policy;

namespace Tenaris.Tamsa.LockVerification.Manager
{
    public class LockVerificationManager : ManagerBase, ILockVerificationManager
    {
        #region Constructor

        /// <summary>
        ///
        /// </summary>
        internal LockVerificationManager()
        {            
            this.DataAccess = new DataAccessClass(this.DbClient);

            this.WarningTime = TimeSpan.Parse(ManagerConfiguration.Settings.WarningTime);
            this.AlarmTime = TimeSpan.Parse(ManagerConfiguration.Settings.AlarmTime);
            this.BlockTime = TimeSpan.Parse(ManagerConfiguration.Settings.BlockTime);
            this.ToleranceTime = TimeSpan.Parse(ManagerConfiguration.Settings.ToleranceTime);

        }

        /// <summary>
        ///
        /// </summary>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Private Attributes

        /// <summary>
        /// 
        /// </summary>
        private DataAccessClass DataAccess;

        /// <summary>
        /// 
        /// </summary>
        private TimeSpan WarningTime;

        /// <summary>
        /// 
        /// </summary>
        private TimeSpan AlarmTime;

        /// <summary>
        /// 
        /// </summary>
        private TimeSpan BlockTime;

        /// <summary>
        /// 
        /// </summary>
        private TimeSpan ToleranceTime;

        /// <summary>
        /// 
        /// </summary>
        private DateTimeOffset NextDateTimeMessage;

        /// <summary>
        /// 
        /// </summary>
        private Thread ThreadVerification;

        /// <summary>
        /// 
        /// </summary>
        private bool AbortThreadVerification;

        /// <summary>
        /// 
        /// </summary>
        private IMailManager mailManagerProxy;

        protected IDictionary<string, string> adaptersInformation;
        #endregion

        #region ILockVerificationManager Members

        /// <summary>
        /// 
        /// </summary>
        public int idLockVerificationHistory
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public IVerification LastVerification
        {
            get;
            set;
        }

        public ILockVerificationHistory LastLockVerificationHistory
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public LockVerificationStatus CurrentStatus
        {
            get;
            set;
        }

        #endregion

        #region IEventNotifier

        /// <summary>
        /// 
        /// </summary>
        private IEventNotifier<LockVerificationStatusChangeEventArgs> lockVerificationStatusChangeNotifier = null;

        /// <summary>
        /// 
        /// </summary>
        private IEventNotifier<LastVerificationChangeEventArgs> lastVerificationChangeNotifier = null;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<LockVerificationStatusChangeEventArgs> OnLockVerificationStatusChange
        {
            add
            {
                this.lockVerificationStatusChangeNotifier.RemoteEvent.Event += value;
                Trace.Warning("Event Lock Verification Status Change Subscribed from {0}", Utility.GetProxyUri(value.Target));
            }
            remove
            {
                this.lockVerificationStatusChangeNotifier.RemoteEvent.Event -= value;
                Trace.Warning("Event Lock Verification Status Change Unsubscribed from {0}", Utility.GetProxyUri(value.Target));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<LastVerificationChangeEventArgs> OnLastVerificationChange
        {
            add
            {
                this.lastVerificationChangeNotifier.RemoteEvent.Event += value;
                Trace.Warning("Event Last Verification Change Subscribed from {0}", Utility.GetProxyUri(value.Target));
            }
            remove
            {
                this.lastVerificationChangeNotifier.RemoteEvent.Event -= value;
                Trace.Warning("Event Last Verification Change Unsubscribed from {0}", Utility.GetProxyUri(value.Target));
            }
        }

        #endregion

        #region ILockVerificationManager Methods

        public TransactionResult SendAuthorization(string user, string password, string comment)
        {
            Trace.Message("Send Authorization...");

            TransactionResult tr = new TransactionResult();

            if (!this.LastLockVerificationHistory.AcceptDateTime.HasValue)
            {
                DateTimeOffset dateNow = DateTimeOffset.Now;

                tr = DataAccess.UpdateLockVerificationHistoryAuthorization(this.LastLockVerificationHistory.idLockVerificationHistory, user, password, comment, dateNow, ManagerConfiguration.Settings.ApplicationCode, ManagerConfiguration.Settings.ApplicationCommand);

                if (tr.Code == Constants.CodeOK)
                {
                    Trace.Message("Send Authorization OK");

                    this.LastLockVerificationHistory.AcceptDateTime = dateNow;
                    this.LastLockVerificationHistory.Comments = Constants.MessageWarning;

                    if (this.CurrentStatus == LockVerificationStatus.Stop)
                    {

                        StringBuilder sb = new StringBuilder();

                        sb.Append("<html><head></head><body>");
                        sb.Append("<p style='color:black; font-family:Verdana; font-size:12pt; font-weight:bold; margin:20px 5px;'>");
                        sb.AppendFormat("Se ha excedido el tiempo máximo de {0} hrs. para la verificación del tubo patrón en {1}</p>", BlockTime.ToString(@"hh\:mm"), ManagerConfiguration.Settings.NameArea);
                        sb.Append("<table border = '0' width = '90%'>");
                        sb.Append("<tr style = 'BORDER: #cccccc 1px solid;color: white;BACKGROUND-COLOR: #336699;font-family: Verdana;font-size: 12pt;font-weight: bold;text-align: center; height:30px;'>");
                        sb.Append("<td colspan = '5'> Ultima Verificación del Tubo Patrón CND</td>");
                        sb.Append("</tr><tr style = 'BORDER: #cccccc 1px solid;COLOR: white;BACKGROUND-COLOR: #CC0066;font-family: Verdana;font-size: 10pt;font-weight: bold;text-align: center;'>");
                        sb.Append("<td> Orden </td><td> Colada </td><td> Tubo </td><td> Tracking </td><td> Fecha Inspección </td ></tr>");
                        sb.Append("<tr style = 'BORDER: #cccccc 1px solid;COLOR: black;BACKGROUND-COLOR: #EEEEEE;font-family: Verdana;font-size: 10pt;text-align: center;'>");
                        sb.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr></table><br><br>", LastVerification.OrderNumber, LastVerification.HeatNumber, LastVerification.TraceabilityNumber, LastVerification.idTracking, LastVerification.InspectionDateTime);
                        sb.Append("<table border='0' width='90%'>");
                        sb.Append("<tr style='BORDER: #cccccc 1px solid;COLOR: white;BACKGROUND-COLOR: #336699;font-family: Verdana;font-size: 12pt;font-weight: bold;text-align: center; height:30px;'>");
                        sb.Append("<td colspan='3'>Autorización de Desbloqueo</td>");
                        sb.Append("</tr><tr style='BORDER: #cccccc 1px solid;COLOR: white;BACKGROUND-COLOR: #CC0066;font-family: Verdana;font-size: 10pt;font-weight: bold;text-align: center;'>");
                        sb.Append("<td width='20%'>Autorizo Matricula</td><td width='30%'>Fecha Autorización</td><td width='50%'>Comentario</td></tr>");
                        sb.Append("<tr style='BORDER: #cccccc 1px solid;COLOR: black;BACKGROUND-COLOR: #EEEEEE;font-family: Verdana;font-size: 10pt;text-align: center;'>");
                        sb.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td></tr></table></body></html>", user, dateNow.ToString("dd/MM/yyyy HH:mm:ss"), comment);



                        mailManagerProxy.SendMail(ManagerConfiguration.Settings.MailAddressesFrom, ManagerConfiguration.Settings.MailAddressesTo, ManagerConfiguration.Settings.MailSubject, sb);
                    }
                }

                return tr;
            }
            else
            {
                Trace.Message("Send Authorization: {0}", Constants.MessageHasAcceptDateTime);

                tr.Code = -992;
                tr.Message = Constants.MessageHasAcceptDateTime;

                return tr;
            }
        }

        public TransactionResult SaveAccept()
        {
            Trace.Message("Save Accept...");

            TransactionResult tr = new TransactionResult();
            
            if (this.CurrentStatus == LockVerificationStatus.Warning && !this.LastLockVerificationHistory.AcceptDateTime.HasValue)
            {
                DateTimeOffset dateNow = DateTimeOffset.Now;

                tr = DataAccess.UpdateLockVerificationHistoryAccept(this.LastLockVerificationHistory.idLockVerificationHistory, Constants.MessageWarning, dateNow);

                if (tr.Code == Constants.CodeOK)
                {
                    Trace.Message("Save Accept OK");

                    this.LastLockVerificationHistory.AcceptDateTime = dateNow;
                    this.LastLockVerificationHistory.Comments = Constants.MessageWarning;
                }

                return tr;

            }
            else
            {
                Trace.Message("Save Accept: {0}", Constants.MessageHasAcceptDateTime);

                tr.Code = -992;
                tr.Message = Constants.MessageHasAcceptDateTime;

                return tr;
            }
        }

        public TransactionResult SaveAcceptAuto()
        {
            Trace.Message("Save Accept in Auto...");

            TransactionResult tr = new TransactionResult();
            //COMMENT
            //if (!this.LastLockVerificationHistory.AcceptDateTime.HasValue)
            //{
            //    DateTimeOffset dateNow = DateTimeOffset.Now;

            //    tr = DataAccess.UpdateLockVerificationHistoryAccept(this.LastLockVerificationHistory.idLockVerificationHistory, Constants.MessageAuto, dateNow);

            //    if (tr.Code == Constants.CodeOK)
            //    {
            //        Trace.Message("Save Accept in Auto OK");

            //        this.LastLockVerificationHistory.AcceptDateTime = dateNow;
            //        this.LastLockVerificationHistory.Comments = Constants.MessageAuto;
            //    }

            //    return tr;

            //}
            //else
            //{
            //    Trace.Message("Save Accept in Auto: {0}", Constants.MessageHasAcceptDateTime);

            //    tr.Code = -992;
            //    tr.Message = Constants.MessageHasAcceptDateTime;

            //    return tr;
            //}

            //COMMENT QUITAR
            return tr;
        }

        public Dictionary<int, string> GetTimeConfiguration()
        {
            Dictionary<int, string> config = new Dictionary<int, string>();

            config.Add((int)LockVerificationStatus.Warning, WarningTime.ToString(@"hh\:mm"));
            config.Add((int)LockVerificationStatus.Alarm, AlarmTime.ToString(@"hh\:mm"));
            config.Add((int)LockVerificationStatus.Stop, BlockTime.ToString(@"hh\:mm"));

            return config;
        }

        #endregion

        #region BaseManager implementation

        /// <summary>
        ///
        /// </summary>
        protected override void DoInitialize()
        {
            try
            {
                Trace.Message("Do Initialize Manager LockVerification...");

                Trace.Message("Time Configuration; Warning: {0}, AlarmTime: {1}, BlockTime: {2}", WarningTime.ToString(@"hh\:mm"), AlarmTime.ToString(@"hh\:mm"), BlockTime.ToString(@"hh\:mm"));

                Trace.Message("Loading EventNotifier...");
                lockVerificationStatusChangeNotifier = new EventNotifier<LockVerificationStatusChangeEventArgs>("OnLockVerificationStatusChange, " + this, this);
                lastVerificationChangeNotifier = new EventNotifier<LastVerificationChangeEventArgs>("OnLastVerificationChange, " + this, this);

                Trace.Message("Loading Synchronization...");
                Synchronization();

                ThreadStart threadStart = new ThreadStart(ThreadVerificationMethod);
                this.ThreadVerification = new Thread(threadStart);
                this.AbortThreadVerification = false;

                Trace.Message("Initialized Mail Manager...");

                ConnectionMonitor.Instance.StateChanged += Instance_StateChanged;

                var managerFactory = FactoryProvider.Instance.CreateFactory<IMailManager>("Tenaris.Manager.Mail");
                mailManagerProxy = managerFactory.Create();

                //Tenaris.Library.System.Factory.IFactory<ICommandManager> factory = Tenaris.Library.System.Factory.FactoryProvider.Instance.CreateFactory<ICommandManager>("Tenaris.Manager.Command.CommandManager");
                //commandManagerProxy = factory.Create();

                Trace.Message("Manager LockVerification Initialized");

            }
            catch (Exception ex)
            {
                Trace.Exception(ex);
            }
        }


        /// <summary>
        ///
        /// </summary>
        protected override void DoActivate()
        {
            try
            {
                Trace.Message("Do Activate Manager LockVerification...");

                this.ThreadVerification.Start();

                Trace.Message("Manager LockVerification Active");

            }
            catch (Exception ex)
            {
                Trace.Exception(ex);
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected override void DoDeactivate()
        {
            try
            {
                Trace.Message("Do Deactivate Manager LockVerification...");

                this.AbortThreadVerification = true;

                Trace.Message("Manager LockVerification Deactive");
            }
            catch (Exception ex)
            {
                Trace.Exception(ex);
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected override void DoUninitialize()
        {
            Trace.Message("Do Uninitialize Manager LockVerification...");

            DataAccess.DoUninitialize();

            this.ThreadVerification.Abort();
            
            Trace.Message("Manager LockVerification DoUninitialized");
        }

        #endregion

        #region Private Methods

        private void Synchronization()
        {
            try
            {
                this.LastVerification = this.DataAccess.GetLastVerification(ManagerConfiguration.Settings.IdTrackingStatus, ManagerConfiguration.Settings.VerificationCount);

                Trace.Warning("Last Verification. IdInspectionHistory: {0}; IdTracking: {1}", this.LastVerification.idInspectionHistory, this.LastVerification.idTracking);


                TimeSpan difference = DateTime.Now - this.LastVerification.InspectionDateTime;

                this.CurrentStatus = LockVerificationStatus.Inactive;
                this.NextDateTimeMessage = this.LastVerification.InspectionDateTime.Add(WarningTime);

                this.LastLockVerificationHistory = this.DataAccess.GetLastLockVerificationHistory();

                bool flagStatusDB = (LastLockVerificationHistory.idLockVerificationHistory > 0) ? true : false;

                Trace.Warning("Last Lock VerificationHistory. IdLockVerificationHistory: {0}; IdStatusLockVerification: {1}; IdLastInspectionHistory: {2}", this.LastLockVerificationHistory.idLockVerificationHistory, this.LastLockVerificationHistory.idStatusLockVerification, this.LastLockVerificationHistory.idLastInspectionHistory);

                if (difference.TotalMinutes >= WarningTime.TotalMinutes && difference.TotalMinutes < AlarmTime.TotalMinutes)
                {
                    this.CurrentStatus = LockVerificationStatus.Warning;
                    this.NextDateTimeMessage = this.LastVerification.InspectionDateTime.Add(AlarmTime);
                }
                else if (difference.TotalMinutes >= AlarmTime.TotalMinutes && difference.TotalMinutes < BlockTime.TotalMinutes)
                {
                    this.CurrentStatus = LockVerificationStatus.Alarm;
                    this.NextDateTimeMessage = this.LastVerification.InspectionDateTime.Add(BlockTime);

                }
                else if (difference.TotalMinutes >= BlockTime.TotalMinutes)
                {
                    this.CurrentStatus = LockVerificationStatus.Stop;                    
                    this.NextDateTimeMessage = DateTime.Now;
                }

                Trace.Warning("Current Status Lock Verification: {0}", this.CurrentStatus.ToString());
                Trace.Warning("Next DateTime Message Lock Verification: {0}", this.NextDateTimeMessage.ToString());

                if (this.CurrentStatus == LastLockVerificationHistory.idStatusLockVerification && this.LastVerification.idInspectionHistory == LastLockVerificationHistory.idLastInspectionHistory)
                {
                    if (LastLockVerificationHistory.AcceptDateTime == null)
                    {
                        //ShowMessageWindow();
                    }
                }
                else
                {
                    //COMMENT
                    //InsertLockVerificationHistory();
                    //ShowMessageWindow();
                }

            }
            catch (Exception ex)
            {
                Trace.Error("Exception in Synchronization, {0}", ex.Message);
            }
        }

        private void ShowMessageWindow()
        {
            Trace.Message("Lock Verification Show Message Window...");

            lockVerificationStatusChangeNotifier.Notify(new LockVerificationStatusChangeEventArgs() { idLockVerificationHistory = this.LastLockVerificationHistory.idLockVerificationHistory, Status = this.CurrentStatus, Message="", Timestamp= DateTime.Now});
        }

        private void InsertLockVerificationHistory()
        {
            try
            {
                Trace.Message("Insert Lock Verification History...");

                DateTimeOffset open = DateTimeOffset.Now;

                int idHistory = DataAccess.InsertLockVerificationHistory((int)this.CurrentStatus, this.LastVerification.idInspectionHistory, open);

                Trace.Message("Insert Lock Verification History; idHistory: {0}", idHistory);

                //Agregar validacion por si no agregar el estado

                this.LastLockVerificationHistory = new LockVerificationHistory();
                this.LastLockVerificationHistory.idLockVerificationHistory = idHistory;
                this.LastLockVerificationHistory.idStatusLockVerification = this.CurrentStatus;
                this.LastLockVerificationHistory.idLastInspectionHistory = this.LastVerification.idInspectionHistory;
                this.LastLockVerificationHistory.OpenDateTime = open;

            }
            catch (Exception ex)
            {
                Trace.Error("Exception in InsertLockVerificationHistory, {0}", ex.Message);
            }
        }

        private void ThreadVerificationMethod()
        {
            try
            {
                int TimerSleep = ManagerConfiguration.Settings.IterationsVerification;

                while (true)
                {
                    try
                    {
                        #region Verification
                        //COMMENT
                        //var TempLastVerification = this.DataAccess.GetLastVerification(ManagerConfiguration.Settings.IdTrackingStatus, ManagerConfiguration.Settings.VerificationCount);
                        Verification TempLastVerification = new Verification();
                        TempLastVerification.HeatNumber = "0";
                        TempLastVerification.InspectionDateTime = DateTimeOffset.Now.AddHours(-4.1);
                        TempLastVerification.OrderNumber = 387841;
                        TempLastVerification.TraceabilityNumber = "";
                        TempLastVerification.idInspectionHistory = 7314588;
                        TempLastVerification.idTracking = 0;
                        TempLastVerification.idTrackingStatus = 0;


                        Trace.Message("ThreadVerificationMethod => Sending Loop, Last Verification: {0}", TempLastVerification.idInspectionHistory);

                        if (TempLastVerification.idInspectionHistory > this.LastVerification.idInspectionHistory)
                        {
                            Trace.Message("ThreadVerificationMethod => Change Last Verification: OldDate: {0}; NewDate: {1}", this.LastVerification.InspectionDateTime, TempLastVerification.InspectionDateTime);

                            bool flagStop = false;

                            if ((this.CurrentStatus == LockVerificationStatus.Stop) && (TempLastVerification.InspectionDateTime > this.LastVerification.InspectionDateTime.Add(ToleranceTime)))
                            {
                                Trace.Message("ThreadVerificationMethod => FlagStop = True, Wait for Authorization...");

                                flagStop = true;
                                this.LastLockVerificationHistory = this.DataAccess.GetLastLockVerificationHistory();
                            }



                            if ((!flagStop) || (flagStop && this.LastLockVerificationHistory.AcceptDateTime != null && this.LastLockVerificationHistory.idUserAuthorization != null))
                            {
                                Trace.Message("ThreadVerificationMethod => Update History for verification; CurrentStatus: {0}; FlagStop: {1}", this.CurrentStatus, flagStop);

                                if (this.LastLockVerificationHistory.AcceptDateTime == null)
                                {
                                    //Actualiza el Historico acceptando la ventana en automatico
                                    //COMMENT
                                    //SaveAcceptAuto();
                                }

                                this.CurrentStatus = LockVerificationStatus.Inactive;
                                this.LastVerification = TempLastVerification;

                                this.NextDateTimeMessage = TempLastVerification.InspectionDateTime.Add(WarningTime);

                                //COMMENT
                                //InsertLockVerificationHistory();

                                //Evento de cambio de verificacion, notificando los datos de la nueva verificacion
                                lastVerificationChangeNotifier.Notify(new LastVerificationChangeEventArgs() { Chronometer = BlockTime.ToString(), LastVerification = this.LastVerification, Timestamp = DateTime.Now });
                            }
                        }
                        else if (this.CurrentStatus != LockVerificationStatus.Stop)
                        {
                            DateTime datetimeNow = DateTime.Now;

                            Trace.Message("ThreadVerificationMethod => Validate time verification; TimeVerification: {0}, TimeNow: {1}", this.NextDateTimeMessage.TimeOfDay, datetimeNow.TimeOfDay);

                            if (this.NextDateTimeMessage.Hour == datetimeNow.Hour)
                            {
                                if (this.NextDateTimeMessage.Minute == datetimeNow.Minute)
                                {

                                    switch(this.CurrentStatus)
                                    {
                                        case LockVerificationStatus.Inactive:
                                            this.CurrentStatus = LockVerificationStatus.Warning;
                                            this.NextDateTimeMessage = this.LastVerification.InspectionDateTime.Add(AlarmTime);
                                            break;
                                        case LockVerificationStatus.Warning:                                            
                                            this.CurrentStatus = LockVerificationStatus.Alarm;
                                            this.NextDateTimeMessage = this.LastVerification.InspectionDateTime.Add(BlockTime);
                                            break;
                                        case LockVerificationStatus.Alarm:                                            
                                            this.CurrentStatus = LockVerificationStatus.Stop;
                                            break;
                                    }

                                    Trace.Message("ThreadVerificationMethod => Active Current Status: {0}, Show Windows", this.CurrentStatus);

                                    //COMMENT
                                    //InsertLockVerificationHistory();
                                    ShowMessageWindow();
                                }
                            }
                        }

                        #endregion

                    }
                    catch (Exception ex)
                    {
                        Trace.Error("Exception in Loop ThreadVerificationMethod, {0}", ex.Message);
                    }

                    if (this.AbortThreadVerification)
                    {
                        Trace.Warning("ThreadVerificationMethod Stop...");
                        System.Threading.Thread.Sleep(60000);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(TimerSleep);
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    Trace.Error("Exception Abort ThreadVerificationMethod...");
                }
                else
                {
                    Trace.Error("Exception in ThreadVerificationMethod, {0}", ex.Message);
                }
            }
        }

        private void Instance_StateChanged(object sender, Tenaris.Library.ConnectionMonitor.StateChangeEventArgs e)
        {
            try
            {
                if (e.IsConnected)
                {
                    Trace.Message("{0}  -> [IMailManager] Connection Monitor; Mail Manager is changed state to ==> [CONNECTED]", DateTime.Now);
                    if (e.Connection is IMailManager)
                    {
                        Console.WriteLine("Connection to Mail Manager");
                        mailManagerProxy = e.Connection as IMailManager;
                    }
                }
                else if (e.IsDisconnected)
                    {
                        Trace.Message("{0} -> [IMailManager] Connection Monitor; Mail Manager is changed state to ==> [DISCONNECTED]", DateTime.Now);
                    }
            }
            catch (Exception ex)
            {
                Trace.Error("Exception in Instance_StateChanged, {0}", ex.Message);
            }
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Tenaris.Library.DbClient;
using Tenaris.Library.Framework;
using Tenaris.Library.Log;
using Tenaris.Tamsa.LockVerification.Manager.Shared;

namespace Tenaris.Tamsa.LockVerification.Manager
{
    public class DataAccessClass
    {
        #region Private Attributes

        /// <summary>
        /// 
        /// </summary>
        private static object mutex = new object();

        /// <summary>
        /// DbClient instance.
        /// </summary>
        private readonly DbClient databaseClient;


        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        internal DataAccessClass(DbClient dbClient)
        {
            try
            {

                // Assign dbclient
                this.databaseClient = dbClient;
                this.databaseClient.AddCommand(StoredProcedures.SpGetLastVerification);
                this.databaseClient.AddCommand(StoredProcedures.SpGetLastLockVerificationHistory);
                this.databaseClient.AddCommand(StoredProcedures.SpInsLockVerificationHistory);
                this.databaseClient.AddCommand(StoredProcedures.SpUpdLockVerificationHistoryAuthorization);
                this.databaseClient.AddCommand(StoredProcedures.SpUpdLockVerificationHistoryAccept);

            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Failed to create DataAccess : {0}", ex.Message);
                throw;
            }
        }

        #endregion

        #region Insert Methods

        public int InsertLockVerificationHistory(int idStatusLockVerification, int idLastInspectionHistory, DateTimeOffset openDateTime)
        {
            lock (mutex)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    int idLockVerificationHistory = 0;

                    try
                    {
                        using (var command = databaseClient.GetCommand(StoredProcedures.SpInsLockVerificationHistory))
                        {
                            Dictionary<string, object> inputParameters = new Dictionary<string, object>();
                            inputParameters.Add(StoredProcedures.Parameters.idStatusLockVerification, idStatusLockVerification);
                            inputParameters.Add(StoredProcedures.Parameters.idLastInspectionHistory, idLastInspectionHistory);
                            inputParameters.Add(StoredProcedures.Parameters.OpenDateTime, openDateTime);

                            ReadOnlyDictionary<string, object> outputParametersRO;

                            //command.Parameters[StoredProcedures.Parameters.idLockVerificationHistory].Direction = System.Data.ParameterDirection.Output;

                            command.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(inputParameters), out outputParametersRO);

                            //if (command.Parameters[StoredProcedures.Parameters.idLockVerificationHistory].Value != null)
                            //{
                            //    idLockVerificationHistory = (int)command.Parameters[StoredProcedures.Parameters.idLockVerificationHistory].Value;
                            //}

                            idLockVerificationHistory = (int)outputParametersRO[StoredProcedures.Fields.idLockVerificationHistory];

                            scope.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Exception(ex, "Error on DataAccess Method: InsertLockVerificationHistory - Parameters:({0},{1},{2})", idStatusLockVerification, idLastInspectionHistory, openDateTime);
                        scope.Dispose();
                    }

                    return idLockVerificationHistory;

                }
            }
        }

        #endregion

        #region Update Methods

        public TransactionResult UpdateLockVerificationHistoryAuthorization(int idLockVerificationHistory, string user, string password, string comments, DateTimeOffset acceptDateTime, string applicationCode, string applicationCommand)
        {
            lock (mutex)
            {
                TransactionResult tr = new TransactionResult();

                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        using (var command = databaseClient.GetCommand(StoredProcedures.SpUpdLockVerificationHistoryAuthorization))
                        {
                            Dictionary<string, object> inputParameters = new Dictionary<string, object>();
                            inputParameters.Add(StoredProcedures.Parameters.idLockVerificationHistory, idLockVerificationHistory);
                            inputParameters.Add(StoredProcedures.Parameters.User, user);
                            inputParameters.Add(StoredProcedures.Parameters.Password, password);
                            inputParameters.Add(StoredProcedures.Parameters.Comments, comments);
                            inputParameters.Add(StoredProcedures.Parameters.AcceptDateTime, acceptDateTime);
                            inputParameters.Add(StoredProcedures.Parameters.ApplicationCode, applicationCode);
                            inputParameters.Add(StoredProcedures.Parameters.ApplicationCommand, applicationCommand);

                            //command.ExecuteNonQuery();

                            using (var rdr = command.ExecuteReader(new ReadOnlyDictionary<string, object>(inputParameters)))
                            {
                                while (rdr.Read())
                                {
                                    tr.Code = int.Parse(rdr[StoredProcedures.Fields.Code].ToString());
                                    tr.Message = rdr[StoredProcedures.Fields.Message].ToString();
                                }

                            }

                            scope.Complete();

                            return tr;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Exception(ex, "Error on DataAccess Method: UpdateLockVerificationHistoryAuthorization - Parameters:({0}, {1}, {2}, {3})", idLockVerificationHistory, user, comments, acceptDateTime);
                        scope.Dispose();

                        tr.Code = -991;
                        tr.Message = string.Format("Error on DataAccess Method: UpdateLockVerificationHistoryAuthorization - Parameters:({0}, {1}, {2}, {3})", idLockVerificationHistory, user, comments, acceptDateTime);

                        return tr;
                    }
                }
            }
        }

        public TransactionResult UpdateLockVerificationHistoryAccept(int idLockVerificationHistory, string comments, DateTimeOffset acceptDateTime)
        {
            lock (mutex)
            {
                TransactionResult tr = new TransactionResult();

                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        using (var command = databaseClient.GetCommand(StoredProcedures.SpUpdLockVerificationHistoryAccept))
                        {
                            Dictionary<string, object> inputParameters = new Dictionary<string, object>();
                            inputParameters.Add(StoredProcedures.Parameters.idLockVerificationHistory, idLockVerificationHistory);
                            inputParameters.Add(StoredProcedures.Parameters.Comments, comments);
                            inputParameters.Add(StoredProcedures.Parameters.AcceptDateTime, acceptDateTime);

                            //command.ExecuteNonQuery();

                            using (var rdr = command.ExecuteReader(new ReadOnlyDictionary<string, object>(inputParameters)))
                            {
                                while (rdr.Read())
                                {
                                    tr.Code = int.Parse(rdr[StoredProcedures.Fields.Code].ToString());
                                    tr.Message = rdr[StoredProcedures.Fields.Message].ToString();
                                }

                            }

                            scope.Complete();

                            return tr;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Exception(ex, "Error on DataAccess Method: UpdateLockVerificationHistoryAuthorization - Parameters:({0}, {1}, {2})", idLockVerificationHistory, comments, acceptDateTime);
                        scope.Dispose();

                        tr.Code = -991;
                        tr.Message = string.Format("Error on DataAccess Method: UpdateLockVerificationHistoryAuthorization - Parameters:({0}, {1}, {2})", idLockVerificationHistory, comments, acceptDateTime);

                        return tr;
                    }
                }
            }
        }

        #endregion

        #region Get Methods

        public Verification GetLastVerification(int idTrackingStatus, int VerificationCount)
        {
            Verification ve = new Verification();

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    using (var command = databaseClient.GetCommand(StoredProcedures.SpGetLastVerification))
                    {
                        Dictionary<string, object> inputParameters = new Dictionary<string, object>();
                        inputParameters.Add(StoredProcedures.Parameters.idTrackingStatus, idTrackingStatus);
                        inputParameters.Add(StoredProcedures.Parameters.VerificationCount, VerificationCount);
                        
                        using (var rdr = command.ExecuteReader(new ReadOnlyDictionary<string, object>(inputParameters)))
                        {
                            while (rdr.Read())
                            {
                                ve.idInspectionHistory = int.Parse(rdr[StoredProcedures.Fields.idInspectionHistory].ToString());
                                //ve.idTrackingStatus = int.Parse(rdr[StoredProcedures.Fields.idTrackingStatus].ToString());
                                ve.OrderNumber = int.Parse(rdr[StoredProcedures.Fields.OrderNumber].ToString());
                                //ve.TraceabilityNumber = rdr[StoredProcedures.Fields.TraceabilityNumber].ToString();
                                ve.HeatNumber = rdr[StoredProcedures.Fields.HeatNumber].ToString();
                                //ve.idTracking = int.Parse(rdr[StoredProcedures.Fields.idTracking].ToString());
                                var dt = rdr[StoredProcedures.Fields.EndDateTime].ToString();
                                ve.InspectionDateTime = dt == String.Empty ? DateTimeOffset.MinValue : DateTime.Parse(rdr[StoredProcedures.Fields.EndDateTime].ToString());
                            }

                        }

                        scope.Complete();
                        return ve;

                    }
                }
                catch (Exception ex)
                {
                    Trace.Exception(ex, "Error on DataAccess Method: GetLastVerification - Parameters:({0}, {1})", idTrackingStatus, VerificationCount);
                    scope.Dispose();
                    return ve;
                }
            }
        }

        public LockVerificationHistory GetLastLockVerificationHistory()
        {
            LockVerificationHistory lvh = new LockVerificationHistory();

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    using (var command = databaseClient.GetCommand(StoredProcedures.SpGetLastLockVerificationHistory))
                    {
                        
                        using (var rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                lvh.idLockVerificationHistory = int.Parse(rdr[StoredProcedures.Fields.idLockVerificationHistory].ToString());
                                lvh.idStatusLockVerification = (LockVerificationStatus)int.Parse(rdr[StoredProcedures.Fields.idStatusLockVerification].ToString());
                                lvh.idLastInspectionHistory = int.Parse(rdr[StoredProcedures.Fields.idLastInspectionHistory].ToString());

                                string temp1 = rdr[StoredProcedures.Fields.idUserAuthorization].ToString();

                                if (!string.IsNullOrEmpty(temp1))
                                {
                                    lvh.idUserAuthorization = int.Parse(temp1);
                                }

                                lvh.idUserLoggedIn = int.Parse(rdr[StoredProcedures.Fields.idUserLoggedIn].ToString());
                                lvh.OpenDateTime = DateTime.Parse(rdr[StoredProcedures.Fields.OpenDateTime].ToString());

                                string temp2 = rdr[StoredProcedures.Fields.AcceptDateTime].ToString();

                                if (!string.IsNullOrEmpty(temp2))
                                {
                                    lvh.AcceptDateTime = DateTime.Parse(temp2);
                                }

                                string temp3 = rdr[StoredProcedures.Fields.idTrackingLock].ToString();

                                if (!string.IsNullOrEmpty(temp3))
                                {
                                    lvh.idTrackingLock = int.Parse(temp3);
                                }

                                string temp4 = rdr[StoredProcedures.Fields.idItemStatusLock].ToString();

                                if (!string.IsNullOrEmpty(temp3))
                                {
                                    lvh.idItemStatusLock = int.Parse(temp4);
                                }

                                lvh.InsertDateTime = DateTime.Parse(rdr[StoredProcedures.Fields.InsDateTime].ToString());
                            }

                        }

                        scope.Complete();
                        return lvh;

                    }
                }
                catch (Exception ex)
                {
                    Trace.Exception(ex, "Error on DataAccess Method: GetLastVerification - Parameters:({0}, {1})");
                    scope.Dispose();
                    return lvh;
                }
            }
        }

        #endregion

        #region Private Methods

        public void DoUninitialize()
        {
            this.databaseClient.ClearCommands();            
            this.databaseClient.Dispose();
        }

        #endregion
    }
}

using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net;

using Npgsql;
using NpgsqlTypes;

using LumiSoft.Net;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IMAP.Server;
using LumiSoft.Net.IMAP;
using LumiSoft.Net.MIME;
using LumiSoft.Net.Mail;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Lumisoft Mail Server Postgre SQL API.
	/// </summary>
	public class pgsql_API : IMailServerApi
	{
		private string m_ConStr = "";

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="intitString"></param>
		public pgsql_API(string intitString)
		{
			// connectionstring=
			string[] parameters = intitString.Replace("\r\n","\n").Split('\n');
			foreach(string param in parameters){
				if(param.ToLower().IndexOf("connectionstring=") > -1){
					m_ConStr = param.Substring(17);
				}
			}
         
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(m_ConStr);
            string database = b.InitialCatalog;
            b.InitialCatalog = "";              
            using(NpgsqlConnection con = new NpgsqlConnection(b.ToString().ToLower().Replace("data source","server"))){                    
                con.Open();

                // See if database exists
                try{
                    con.ChangeDatabase(database);
                }
                catch{
                    // Database don't exist, try to create it

                    try{
                        con.Close();
                        con.ConnectionString = b.ToString().ToLower().Replace("data source","server");
                        con.Open();


                        NpgsqlCommand cmd = new NpgsqlCommand();
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "create database \"" + database + "\"";
                        cmd.ExecuteNonQuery();
                        con.ChangeDatabase(database);

                        // Create tables
                        cmd.CommandText = ResManager.GetText("tables.sql",System.Text.Encoding.Default);
                        cmd.ExecuteNonQuery();

                        // Create procedures
                        cmd.CommandText = ResManager.GetText("procedures.sql",System.Text.Encoding.Default);
                        cmd.ExecuteNonQuery();
                    }
                    catch{
                        throw new Exception("Database '" + database + "' doesn''t exist ! Create failed, specified user doesn't have enough permisssions to create database ! Create database manually.");
                    }                    
                }
            }
		}


		#region Domain related

		#region method GetDomains

		/// <summary>
		/// Gets domain list.
		/// </summary>
		/// <returns></returns>
		public DataView GetDomains()
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetDomains")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Domains";

				return ds.Tables["Domains"].DefaultView;
			}
		}

		#endregion


		#region method AddDomain

		/// <summary>
		/// Adds new domain.
		/// </summary>
		/// <param name="domainID">Domain ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="domainName">Domain name. Eg. yourDomain.com .</param>
		/// <param name="description">Domain description.</param>
		/// <remarks>Throws exception if specified domain already exists.</remarks>
		public void AddDomain(string domainID,string domainName,string description)
		{
			if(domainID.Length == 0){
				throw new Exception("You must specify domainID");
			}
			ArgsValidator.ValidateDomainName(domainName);

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddDomain")){
				sqlCmd.AddParameter("_domainID"   ,NpgsqlDbType.Varchar,domainID);
				sqlCmd.AddParameter("_domainName" ,NpgsqlDbType.Varchar,domainName);
				sqlCmd.AddParameter("_description",NpgsqlDbType.Varchar,description);

                DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method DeleteDomain

		/// <summary>
		/// Deletes specified domain.
		/// </summary>
		/// <param name="domainID">Domain ID. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <remarks>Deletes specified domain and all domain related data (users,mailing lists,routes).</remarks>
		public void DeleteDomain(string domainID)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteDomain")){
				sqlCmd.AddParameter("_domainID" ,NpgsqlDbType.Varchar,domainID);
							
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

        #region method UpdateDomain

        /// <summary>
        /// Updates specified domain data.
        /// </summary>
        /// <param name="domainID">Domain ID which to update.</param>
        /// <param name="domainName">Domain name.</param>
        /// <param name="description">Domain description.</param>
        public void UpdateDomain(string domainID,string domainName,string description)
        {
            if(domainID.Length == 0){
				throw new Exception("You must specify domainID");
			}
            ArgsValidator.ValidateDomainName(domainName);

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateDomain")){
				sqlCmd.AddParameter("_domainID"   ,NpgsqlDbType.Varchar,domainID);
				sqlCmd.AddParameter("_domainName" ,NpgsqlDbType.Varchar,domainName);
				sqlCmd.AddParameter("_description",NpgsqlDbType.Varchar,description);

                DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

		#region method DomainExists

		/// <summary>
		/// Checks if specified domain exists.
		/// </summary>
		/// <param name="source">Domain name or email address.</param>
		/// <returns>Returns true if domain exists.</returns>
		public bool DomainExists(string source)
		{
			// Source is Emails
			if(source.IndexOf("@") > -1){
				source = source.Substring(source.IndexOf("@") + 1);
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DomainExists")){
				sqlCmd.AddParameter("_domainName",NpgsqlDbType.Varchar,source);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Domains";

				if(ds.Tables["Domains"].Rows.Count > 0){
					return true;
				}
			}

			return false;
		}

		#endregion

		#endregion


		#region User and Groups related

		#region method GetUsers

		/// <summary>
		/// Gets user list in specified domain.
		/// </summary>
		/// <param name="domainName">Domain which user list to retrieve.To get all use value 'ALL'.</param>
		/// <returns></returns>
		public DataView GetUsers(string domainName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUsers")){
				if(domainName != "ALL"){
					sqlCmd.AddParameter("_domainName",NpgsqlDbType.Varchar,domainName);
				}

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";

				return ds.Tables["Users"].DefaultView;
			}
		}

		#endregion


        #region method GetUserID

        /// <summary>
        /// Gets user ID from user name. Returns null if user doesn't exist.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Returns user ID or null if user doesn't exist.</returns>
        public string GetUserID(string userName)
        {   
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"select UserID from lsUsers where (UserName = '" + userName + "')")){				
                sqlCmd.CommandType = CommandType.Text;
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";

                if(ds.Tables["Users"].Rows.Count > 0){
                    return ds.Tables["Users"].Rows[0]["UserID"].ToString();
                }
			}
     
            return null;
        }

        #endregion

		#region method AddUser

		/// <summary>
		/// Adds new user to specified domain.
		/// </summary>
		/// <param name="userID">User ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="userName">User login name.</param>
		/// <param name="fullName">User full name.</param> 
		/// <param name="password">User login password.</param>
		/// <param name="description">User description.</param>
		/// <param name="domainName">Domain where to add user. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <param name="mailboxSize">Maximum mailbox size.</param>
		/// <param name="enabled">Sepcifies if user is enabled.</param>
		/// <param name="permissions">Specifies user permissions.</param>
		/// <remarks>Throws exception if specified user already exists.</remarks>
		public void AddUser(string userID,string userName,string fullName,string password,string description,string domainName,int mailboxSize,bool enabled,UserPermissions_enum permissions)
		{
			if(userID.Length == 0){
				throw new Exception("You must specify userID");
			}
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddUser")){
				sqlCmd.AddParameter("_userID"      ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_fullName"    ,NpgsqlDbType.Varchar,fullName);
				sqlCmd.AddParameter("_userName"    ,NpgsqlDbType.Varchar,userName);
				sqlCmd.AddParameter("_password"    ,NpgsqlDbType.Varchar,password);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_domainName"  ,NpgsqlDbType.Varchar,domainName);
				sqlCmd.AddParameter("_mailboxSize" ,NpgsqlDbType.Integer,mailboxSize);
				sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_permissions" ,NpgsqlDbType.Integer,permissions);
							
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";
			}			
		}
	
		#endregion

		#region method DeleteUser

		/// <summary>
		/// Deletes user.
		/// </summary>
		/// <param name="userID">User id of the user which to delete. Use <see cref="IMailServerApi.GetUsers">>GetUsers()</see> to get valid values.</param>
		public void DeleteUser(string userID)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteUser")){
				sqlCmd.AddParameter("_userID" ,NpgsqlDbType.Varchar,userID);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method UpdateUser

		/// <summary>
		/// Updates new user to specified domain.
		/// </summary>
		/// <param name="userID">User id of the user which to update. Use <see cref="IMailServerApi.GetUsers">>GetUsers()</see> to get valid values.</param>
		/// <param name="userName">User login name.</param>
		/// <param name="fullName">User full name.</param>
		/// <param name="password">User login password.</param>
		/// <param name="description">User description.</param>
		/// <param name="domainName">Domain where to add user. Use <see cref="IMailServerApi.GetDomains">>GetDomains()</see> to get valid values.</param>
		/// <param name="mailboxSize">Maximum mailbox size.</param>
		/// <param name="enabled">Sepcifies if user is enabled.</param>
		/// <param name="permissions">Specifies user permissions.</param>
		public void UpdateUser(string userID,string userName,string fullName,string password,string description,string domainName,int mailboxSize,bool enabled,UserPermissions_enum permissions)
		{
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateUser")){
				sqlCmd.AddParameter("_userID"      ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_userName"    ,NpgsqlDbType.Varchar,userName);
				sqlCmd.AddParameter("_fullName"    ,NpgsqlDbType.Varchar,fullName);
				sqlCmd.AddParameter("_password"    ,NpgsqlDbType.Varchar,password);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_domainName"  ,NpgsqlDbType.Varchar,domainName);
				sqlCmd.AddParameter("_mailboxSize" ,NpgsqlDbType.Integer,mailboxSize);
				sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_permissions" ,NpgsqlDbType.Integer,permissions);
							
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";
			}
		}
		
		#endregion

		#region method AddUserAddress

		/// <summary>
		/// Add new email address to user.
		/// </summary>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">>GetUsers()</see> to get valid values.</param>
		/// <param name="emailAddress">Email address to add.</param>
		/// <remarks>Throws exception if specified user email address exists.</remarks>
		public void AddUserAddress(string userName,string emailAddress)
		{
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}
			if(emailAddress.Length == 0){
				throw new Exception("You must specify address");
			}            
			if(emailAddress.IndexOf('@') == -1){
				throw new Exception("Invalid email address, @ is missing !");
			}

            string[] localPart_domain = emailAddress.Split('@');

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddEmailAddress")){
				sqlCmd.AddParameter("_localPart" ,NpgsqlDbType.Varchar,localPart_domain[0]);
				sqlCmd.AddParameter("_domainName",NpgsqlDbType.Varchar,localPart_domain[1]);
				sqlCmd.AddParameter("_OwnerID"   ,NpgsqlDbType.Varchar,GetUserID(userName));
							
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method DeleteUserAddress

		/// <summary>
		/// Deletes specified email address from user. 
		/// </summary>
		/// <param name="emailAddress">Email address to delete.</param>
		public void DeleteUserAddress(string emailAddress)
		{
            if(emailAddress.Length == 0){
				throw new Exception("You must specify address");
			}            
			if(emailAddress.IndexOf('@') == -1){
				throw new Exception("Invalid email address, @ is missing !");
			}

            string[] localPart_domain = emailAddress.Split('@');

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteEmailAddress")){
				sqlCmd.AddParameter("_localPart" ,NpgsqlDbType.Varchar,localPart_domain[0]);
				sqlCmd.AddParameter("_domainName",NpgsqlDbType.Varchar,localPart_domain[1]);
							
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method GetUserAddresses

		/// <summary>
		/// Gets user email addresses.
		/// </summary>
		/// <param name="userName"> Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		public DataView GetUserAddresses(string userName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserAddresses")){
				sqlCmd.AddParameter("@UserName",NpgsqlDbType.Varchar,userName);
				
				DataSet ds = sqlCmd.Execute();

                DataSet dsRetVal = new DataSet();
                dsRetVal.Tables.Add("UserAddresses");
                dsRetVal.Tables["UserAddresses"].Columns.Add("Address");
                foreach(DataRow dr in ds.Tables[0].Rows){
                    DataRow drNew = dsRetVal.Tables["UserAddresses"].NewRow();
                    drNew["Address"] = dr[0].ToString();
                    dsRetVal.Tables["UserAddresses"].Rows.Add(drNew);
                }

				return dsRetVal.Tables["UserAddresses"].DefaultView;
			}
		}

		#endregion

		#region method UserExists

		/// <summary>
		/// Checks if user exists.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns>Returns true if user exists.</returns>
		public bool UserExists(string userName)
		{			
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserProperties")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";

				if(ds.Tables["Users"].Rows.Count > 0){
					return true;
				}
			}

			return false;
		}
		
		#endregion

		#region method MapUser

		/// <summary>
		/// Maps email address to mailbox.
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns>Returns mailbox or null if such email address won't exist.</returns>
		public string MapUser(string emailAddress)
		{
            if(emailAddress.Length == 0){
				return null;
			}            
			if(emailAddress.IndexOf('@') == -1){
				return null;
			}

            string[] localPart_domain = emailAddress.Split('@');

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_MapUser")){
				sqlCmd.AddParameter("_localPart" ,NpgsqlDbType.Varchar,localPart_domain[0]);
				sqlCmd.AddParameter("_domainName",NpgsqlDbType.Varchar,localPart_domain[1]);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "UserAddresses";

				if(ds.Tables["UserAddresses"].Rows.Count > 0){
					return ds.Tables["UserAddresses"].Rows[0]["UserName"].ToString();
				}
			}

			return null;
		}
		
		#endregion

		#region method ValidateMailboxSize

		/// <summary>
		/// Checks if specified mailbox size is exceeded.
		/// </summary>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <returns>Returns true if exceeded.</returns>
		public bool ValidateMailboxSize(string userName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_ValidateMailboxSize")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				DataSet ds = sqlCmd.Execute();
                return (bool)sqlCmd.ExecuteScalar();
			}
		}
		
		#endregion

        #region method GetUserPermissions

        /// <summary>
        /// Gets specified user permissions.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns></returns>
        public UserPermissions_enum GetUserPermissions(string userName)
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserProperties")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";

				if(ds.Tables["Users"].Rows.Count > 0){
					return (UserPermissions_enum)Convert.ToInt32(ds.Tables["Users"].Rows[0]["Permissions"]);
				}
			}

            return UserPermissions_enum.None;
        }

        #endregion

        #region method GetUserLastLoginTime

        /// <summary>
        /// Gets user last login time.
        /// </summary>
        /// <param name="userName">User name who's last login time to get.</param>
        /// <returns>User last login time.</returns>
        public DateTime GetUserLastLoginTime(string userName)
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserProperties")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";

				if(ds.Tables["Users"].Rows.Count > 0){
					return Convert.ToDateTime(ds.Tables["Users"].Rows[0]["LastLoginTime"]);
				}

                return DateTime.Now;
			}
        }

        #endregion

        #region method UpdateUserLastLoginTime

        /// <summary>
        /// Updates user last login time.
        /// </summary>
        /// <param name="userName">User name who's last login time to update.</param>
        public void UpdateUserLastLoginTime(string userName)
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateUserLastLoginTime")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion


        #region method GetUserRemoteServers

		/// <summary>
		/// Gets user pop3 remote accounts.
		/// </summary>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <returns></returns>
		public DataView GetUserRemoteServers(string userName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserRemoteServers")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "UserRemoteServers";

				return ds.Tables["UserRemoteServers"].DefaultView;
			}
		}

		#endregion

		#region method AddUserRemoteServer

		/// <summary>
		/// AAdds new remote pop3 server to user.
		/// </summary>
		/// <param name="serverID">Server ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <param name="description">Remote server description.</param>
		/// <param name="remoteServer">Remote server name.</param>
		/// <param name="remotePort">Remote server port.</param>
		/// <param name="remoteUser">Remote server user name.</param>
		/// <param name="remotePassword">Remote server password.</param>
		/// <param name="useSSL">Specifies if SSL must be used to connect to remote server.</param>
		/// <param name="enabled">Specifies if remote server is enabled.</param>
		/// <remarks>Throws exception if specified user remote server already exists.</remarks>
		public void AddUserRemoteServer(string serverID,string userName,string description,string remoteServer,int remotePort,string remoteUser,string remotePassword,bool useSSL,bool enabled)
		{
			if(serverID.Length == 0){
				throw new Exception("You must specify serverID");
			}
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddUserRemoteServer")){
				sqlCmd.AddParameter("_serverID"       ,NpgsqlDbType.Varchar,serverID);
				sqlCmd.AddParameter("_userName"       ,NpgsqlDbType.Varchar,userName);
                sqlCmd.AddParameter("_description"    ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_remoteServer"   ,NpgsqlDbType.Varchar,remoteServer);
				sqlCmd.AddParameter("_remotePort"     ,NpgsqlDbType.Integer,remotePort);
				sqlCmd.AddParameter("_remoteUserName" ,NpgsqlDbType.Varchar,remoteUser);
				sqlCmd.AddParameter("_remotePassword" ,NpgsqlDbType.Varchar,remotePassword);
                sqlCmd.AddParameter("_useSSL"         ,NpgsqlDbType.Boolean,useSSL);
                sqlCmd.AddParameter("_enabled"        ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method DeleteUserRemoteServer

		/// <summary>
		/// Deletes specified pop3 remote account from user.
		/// </summary>
		/// <param name="serverID">Remote server ID. Use <see cref="IMailServerApi.GetUserRemoteServers">GetUserRemoteServers()</see> to get valid values.</param>
		public void DeleteUserRemoteServer(string serverID)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteUserRemoteServer")){
				sqlCmd.AddParameter("_serverID" ,NpgsqlDbType.Varchar,serverID);
				
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

        #region mehtod UpdateUserRemoteServer

        /// <summary>
		/// Updates user remote pop3 server.
		/// </summary>
		/// <param name="serverID">Server ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <param name="description">Remote server description.</param>
		/// <param name="remoteServer">Remote server name.</param>
		/// <param name="remotePort">Remote server port.</param>
		/// <param name="remoteUser">Remote server user name.</param>
		/// <param name="remotePassword">Remote server password.</param>
		/// <param name="useSSL">Specifies if SSL must be used to connect to remote server.</param>
		/// <param name="enabled">Specifies if remote server is enabled.</param>
		/// <remarks>Throws exception if specified user remote server already exists.</remarks>
		public void UpdateUserRemoteServer(string serverID,string userName,string description,string remoteServer,int remotePort,string remoteUser,string remotePassword,bool useSSL,bool enabled)
        {
            if(serverID.Length == 0){
				throw new Exception("You must specify serverID");
			}
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateUserRemoteServer")){
				sqlCmd.AddParameter("_serverID"       ,NpgsqlDbType.Varchar,serverID);
				sqlCmd.AddParameter("_userName"       ,NpgsqlDbType.Varchar,userName);
                sqlCmd.AddParameter("_description"    ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_remoteServer"   ,NpgsqlDbType.Varchar,remoteServer);
				sqlCmd.AddParameter("_remotePort"     ,NpgsqlDbType.Integer,remotePort);
				sqlCmd.AddParameter("_remoteUserName" ,NpgsqlDbType.Varchar,remoteUser);
				sqlCmd.AddParameter("_remotePassword" ,NpgsqlDbType.Varchar,remotePassword);
                sqlCmd.AddParameter("_useSSL"         ,NpgsqlDbType.Boolean,useSSL);
                sqlCmd.AddParameter("_enabled"        ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

		
		#region method GetUserMessageRules

		/// <summary>
		/// Gets user message  rules.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns></returns>
		public DataView GetUserMessageRules(string userName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserMessageRules")){
				if(userName.Length > 0){
					sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);
				}

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "UserMessageRules";

                // We need to convert MatchExpression to string. 
                // PGSQL won't allow strings bigger than 4000 chars, because of it we store it as Bytea.
                ds.Tables[0].Columns["MatchExpression"].ColumnName = "MatchExpressionOld";
                ds.Tables[0].Columns.Add("MatchExpression",typeof(string));
                foreach(DataRow dr in ds.Tables[0].Rows){
                    dr["MatchExpression"] = System.Text.Encoding.Default.GetString((byte[])dr["MatchExpressionOld"]);
                }
                ds.Tables[0].Columns.Remove("MatchExpressionOld");

				return ds.Tables["UserMessageRules"].DefaultView;
			}
		}

		#endregion

		#region method AddUserMessageRule

		/// <summary>
        /// Adds new user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void AddUserMessageRule(string userID,string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddUserMessageRule")){
                sqlCmd.AddParameter("_userID"          ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_ruleID"          ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_cost"            ,NpgsqlDbType.Bigint  ,cost);
				sqlCmd.AddParameter("_enabled"         ,NpgsqlDbType.Boolean ,enabled);
				sqlCmd.AddParameter("_checkNextRule"   ,NpgsqlDbType.Integer ,checkNextRule);
				sqlCmd.AddParameter("_description"     ,NpgsqlDbType.Varchar,description);
                sqlCmd.AddParameter("_matchExpression" ,NpgsqlDbType.Bytea   ,System.Text.Encoding.Default.GetBytes(matchExpression));
							
				DataSet ds = sqlCmd.Execute();
			}
        }

		#endregion

		#region method DeleteUserMessageRule

		/// <summary>
		/// Deletes specified user message rule.
		/// </summary>
        /// <param name="userID">User who owns specified rule.</param>
		/// <param name="ruleID">Rule ID.</param>
		public void DeleteUserMessageRule(string userID,string ruleID)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteUserMessageRule")){
                sqlCmd.AddParameter("_userID" ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_ruleID" ,NpgsqlDbType.Varchar,ruleID);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

		#endregion

		#region method UpdateUserMessageRule

		/// <summary>
        /// Updates specified user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void UpdateUserMessageRule(string userID,string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateUserMessageRule")){
                sqlCmd.AddParameter("_userID"          ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_ruleID"          ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_cost"            ,NpgsqlDbType.Bigint  ,cost);
				sqlCmd.AddParameter("_enabled"         ,NpgsqlDbType.Boolean ,enabled);
				sqlCmd.AddParameter("_checkNextRule"   ,NpgsqlDbType.Integer ,checkNextRule);
				sqlCmd.AddParameter("_description"     ,NpgsqlDbType.Varchar ,description);
                sqlCmd.AddParameter("_matchExpression" ,NpgsqlDbType.Bytea   ,System.Text.Encoding.Default.GetBytes(matchExpression));
							
				DataSet ds = sqlCmd.Execute();
			}
        }

		#endregion

        #region method GetUserMessageRuleActions

        /// <summary>
        /// Gets specified user message rule actions.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID of rule which actions to get.</param>
        public DataView GetUserMessageRuleActions(string userID,string ruleID)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserMessageRuleActions")){
                sqlCmd.AddParameter("_userID",NpgsqlDbType.Varchar,userID);
			    sqlCmd.AddParameter("_ruleID",NpgsqlDbType.Varchar,ruleID);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "UserMessageRuleActions";

				return ds.Tables["UserMessageRuleActions"].DefaultView;
			}
        }

        #endregion

        #region method AddUserMessageRuleAction

        /// <summary>
        /// Adds action to specified user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID to which to add this action.</param>
        /// <param name="actionID">Action ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void AddUserMessageRuleAction(string userID,string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddUserMessageRuleAction")){
                sqlCmd.AddParameter("_userID"      ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_ruleID"      ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_actionID"    ,NpgsqlDbType.Varchar,actionID);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_actionType"  ,NpgsqlDbType.Integer,(int)actionType);
                sqlCmd.AddParameter("_actionData"  ,NpgsqlDbType.Bytea  ,actionData);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method DeleteUserMessageRuleAction

        /// <summary>
        /// Deletes specified action from specified user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID which action to delete.</param>
        /// <param name="actionID">Action ID of action which to delete.</param>
        public void DeleteUserMessageRuleAction(string userID,string ruleID,string actionID)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteUserMessageRuleAction")){
                sqlCmd.AddParameter("_userID"   ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_ruleID"   ,NpgsqlDbType.Varchar,ruleID);
                sqlCmd.AddParameter("_actionID" ,NpgsqlDbType.Varchar,actionID);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method UpdateUserMessageRuleAction

        /// <summary>
        /// Updates specified rule action.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID which action to update.</param>
        /// <param name="actionID">Action ID of action which to update.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void UpdateUserMessageRuleAction(string userID,string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateUserMessageRuleAction")){
                sqlCmd.AddParameter("_userID"      ,NpgsqlDbType.Varchar,userID);
				sqlCmd.AddParameter("_ruleID"      ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_actionID"    ,NpgsqlDbType.Varchar,actionID);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_actionType"  ,NpgsqlDbType.Integer     ,(int)actionType);
                sqlCmd.AddParameter("_actionData"  ,NpgsqlDbType.Bytea   ,actionData);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion


		#region method AuthUser

		/// <summary>
		/// Authenticates user.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <param name="passwData">Password data.</param>
		/// <param name="authData">Authentication specific data(as tag).</param>
		/// <param name="authType">Authentication type.</param>
		/// <returns></returns>
		public DataSet AuthUser(string userName,string passwData,string authData,AuthType authType)
		{
			DataSet retVal = new DataSet();
			DataTable dt = retVal.Tables.Add("Result");
			dt.Columns.Add("Result");
			dt.Columns.Add("ReturnData");
			DataRow drx = dt.NewRow();
			drx["Result"] = "false";
			drx["ReturnData"] = "";
			dt.Rows.Add(drx);

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserProperties")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);
						
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Users";

				if(ds.Tables["Users"].Rows.Count > 0){
					string password = ds.Tables["Users"].Rows[0]["PASSWORD"].ToString().ToLower();

					switch(authType)
					{
						case AuthType.APOP:
							if(AuthHelper.Apop(password,authData) == passwData){
								drx["Result"] = "true";
								return retVal;
							}
							break;

						case AuthType.CRAM_MD5:	
							if(AuthHelper.Cram_Md5(password,authData) == passwData){
								drx["Result"] = "true";
								return retVal;
							}
							break;

						case AuthType.DIGEST_MD5:	
							Auth_HttpDigest digest = new Auth_HttpDigest(authData,"AUTHENTICATE");
                            if(digest.Authenticate(userName,password)){
                                drx["Result"]     = "true";
								drx["ReturnData"] = digest.CalculateResponse(userName,password);
                                                      
								return retVal;
                            }
							break;

						case AuthType.Plain:
							if(password == passwData.ToLower()){
								drx["Result"] = "true";
								return retVal;
							}
							break;						
					}
				}
			}			

			return retVal;
		}
		
		#endregion


        #region method GroupExists

        /// <summary>
        /// Gets if specified user group exists.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <returns>Returns true, if user group exists.</returns>
        public bool GroupExists(string groupName)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Check if group exists.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            //----------------------------------------//

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GroupExists")){
				sqlCmd.AddParameter("_groupName",NpgsqlDbType.Varchar,groupName);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Result";

				if(ds.Tables["Result"].Rows.Count > 0){
					return true;
				}
			}

			return false;
        }

        #endregion

        #region method GetGroups

        /// <summary>
        /// Gets user groups.
        /// </summary>
        /// <returns></returns>
        public DataView GetGroups()
        {
            /* Implementation notes:
                *) Get groups.
            */

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetGroups")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Groups";

				return ds.Tables["Groups"].DefaultView;
			}
        }

        #endregion

        #region method AddGroup

        /// <summary>
        /// Adds new user group.
        /// </summary>
        /// <param name="groupID">Group ID. Guid.NewGuid().ToString() is suggested.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="description">Group description.</param>
        /// <param name="enabled">Specifies if group is enabled.</param>
        public void AddGroup(string groupID,string groupName,string description,bool enabled)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group ID won't exist already. Throw Exception if does.
                *) Ensure that group or user with specified name doesn't exist. Throw Exception if does.
                *) Add group.
            */

            //--- Validate values --------------------//
            if(groupID == null || groupID == ""){
                throw new Exception("Invalid groupID value, groupID can't be '' or null !");
            }
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateNotNull(description);
            //----------------------------------------//

            /* We handle these is SQL, sql throws Excption
             
                *) Ensure that group ID won't exist already.
                *) Ensure that group or user with specified name doesn't exist.
             
            */

            // Insert group
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddGroup")){
				sqlCmd.AddParameter("_groupID"     ,NpgsqlDbType.Varchar,groupID);
				sqlCmd.AddParameter("_groupName"   ,NpgsqlDbType.Varchar,groupName);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Groups";
			}
        }
       
        #endregion

        #region method DeleteGroup

        /// <summary>
        /// Deletes specified user group.
        /// </summary>
        /// <param name="groupID">Group ID.</param>
        public void DeleteGroup(string groupID)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group does exist.  Throw Exception if doesn't.
                *) Delete group members.
                *) Delete group.
            */

            //--- Validate values --------------------//
            if(groupID == null || groupID == ""){
                throw new Exception("Invalid groupID value, groupID can't be '' or null !");
            }
            //----------------------------------------//

            /* We handle these is SQL, sql throws Excption
             
                *) Ensure that group does exist.
                *) Delete group members.
             
            */

            // Delete group and it's emebres
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteGroup")){
				sqlCmd.AddParameter("_groupID" ,NpgsqlDbType.Varchar,groupID);
							
				DataSet ds = sqlCmd.Execute();
                ds.Tables[0].TableName = "Groups";
			}
        }

        #endregion

        #region method UpdateGroup

        /// <summary>
        /// Updates user group info.
        /// </summary>
        /// <param name="groupID">Group ID.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="description">Group description.</param>
        /// <param name="enabled">Specifies if group is enabled.</param>
        public void UpdateGroup(string groupID,string groupName,string description,bool enabled)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group with specified ID does exist.  Throw Exception if doesn't.
                *) If group name is changed, ensure that new group name won't conflict 
                   any other group or user name. Throw Exception if does.                    
                *) Udpate group.
            */

            //--- Validate values --------------------//
            if(groupID == null || groupID == ""){
                throw new Exception("Invalid groupID value, groupID can't be '' or null !");
            }
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateNotNull(description);
            //----------------------------------------//

            /* We handle these is SQL, sql throws Excption
             
            *) Ensure that group with specified ID does exist.  Throw Exception if doesn't.
            *) If group name is changed, ensure that new group name won't conflict 
               any other group or user name. Throw Exception if does.
            */

            // Update group
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateGroup")){
				sqlCmd.AddParameter("_groupID"     ,NpgsqlDbType.Varchar,groupID);
				sqlCmd.AddParameter("_groupName"   ,NpgsqlDbType.Varchar,groupName);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Groups";
			}
        }
        
        #endregion

        #region method GroupMemberExists

        /// <summary>
        /// Gets if specified group member exists in specified user group members list.
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="userOrGroup">User or group.</param>
        /// <returns></returns>
        public bool GroupMemberExists(string groupName,string userOrGroup)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Check if group member exists.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateUserName(userOrGroup);
            //----------------------------------------//

            // Ensure that group exists. Throw Exception if doesn't.
            if(!GroupExists(groupName)){
                throw new Exception("Invalid group name, specified group '" + groupName + "' doesn't exist !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GroupMemberExists")){
				sqlCmd.AddParameter("_groupName"  ,NpgsqlDbType.Varchar,groupName);
                sqlCmd.AddParameter("_userOrGroup",NpgsqlDbType.Varchar,userOrGroup);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Result";

				if(ds.Tables["Result"].Rows.Count > 0){
					return true;
				}
			}

            return false;
        }

        #endregion

        #region method GetGroupMembers

        /// <summary>
        /// Gets useer group members who belong to specified group.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <returns></returns>
        public string[] GetGroupMembers(string groupName)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Get members.
            */

            //--- Validate values --------------------//
            if(groupName == null || groupName == ""){
                throw new Exception("Invalid groupName value, groupName can't be '' or null !");
            }
            //----------------------------------------//

            // Ensure that group exists.
            if(!GroupExists(groupName)){
                throw new Exception("Invalid group name, specified group name '" + groupName + "' doesn't exist !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetGroupMembers")){
                sqlCmd.AddParameter("_groupName" ,NpgsqlDbType.Varchar,groupName);
                             
				DataSet ds = sqlCmd.Execute();

                List<string> members = new List<string>();
                foreach(DataRow dr in ds.Tables[0].Rows){
                    members.Add(dr[0].ToString());
                }
				return members.ToArray();
			}
        }

        #endregion

        #region method AddGroupMember

        /// <summary>
        /// Add specified user or group to specified goup members list.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <param name="userOrGroup">User or group.</param>
        public void AddGroupMember(string groupName,string userOrGroup)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Don't allow to add same group as group member.
                *) Ensure that group member doesn't exist. Throw Exception if does.
                *) Add group member.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateUserName(userOrGroup);
            //----------------------------------------//

            /* We handle these is SQL, sql returns these errors in ErrorText
            
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Don't allow to add same group as group member.
                *) Ensure that group member doesn't exist. Throw Exception if does.
            */

            // Add group member
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddGroupMember")){
				sqlCmd.AddParameter("_groupName"   ,NpgsqlDbType.Varchar,groupName);
				sqlCmd.AddParameter("_userOrGroup" ,NpgsqlDbType.Varchar,userOrGroup);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method DeleteGroupMember

        /// <summary>
        /// Deletes specified user or group from specified group members list.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <param name="userOrGroup">User or group.</param>
        public void DeleteGroupMember(string groupName,string userOrGroup)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Ensure that group member does exist. Throw Exception if doesn't.
                *) Delete group member.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateUserName(userOrGroup);
            //----------------------------------------//

            /* We handle these is SQL, sql returns these errors in ErrorText
            
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Ensure that group member does exist. Throw Exception if doesn't.
              
            */            

            // Delete group and it's emebres
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteGroupMember")){
				sqlCmd.AddParameter("_groupName"   ,NpgsqlDbType.Varchar,groupName);
                sqlCmd.AddParameter("_userOrGroup" ,NpgsqlDbType.Varchar,userOrGroup);
							
				DataSet ds = sqlCmd.Execute();
			}                       
        }

        #endregion

        #region method GetGroupUsers

        /// <summary>
        /// Gets specified group users. All nested group members are replaced by actual users.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <returns></returns>
        public string[] GetGroupUsers(string groupName)
        {
            List<string> users = new List<string>();
            List<string> proccessedGroups = new List<string>();
            Queue<string> membersQueue = new Queue<string>();
            string[] members = GetGroupMembers(groupName);
            foreach(string member in members){
                membersQueue.Enqueue(member);
            }

            while(membersQueue.Count > 0){
                string member = membersQueue.Dequeue();
                // Nested group
                DataRow drGroup = GetGroup(member);
                if(drGroup != null){
                    // Don't proccess poroccessed groups any more, causes infinite loop
                    if(!proccessedGroups.Contains(member.ToLower())){
                        // Skip disabled groups
                        if(Convert.ToBoolean(drGroup["Enabled"])){
                            members = GetGroupMembers(member);
                            foreach(string m in members){
                                membersQueue.Enqueue(m);
                            }  
                        }

                        proccessedGroups.Add(member.ToLower());
                    }
                }
                // User
                else{
                    if(!users.Contains(member)){
                        users.Add(member);
                    }
                }
            }

            return users.ToArray();
        }

        #endregion

		#endregion


		#region MailingList related
		
		#region method GetMailingLists

		/// <summary>
		/// Gets mailing lists.
		/// </summary>
		/// <param name="domainName">Domain name. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <returns></returns>
		public DataView GetMailingLists(string domainName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMailingLists")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "MailingLists";

				return ds.Tables["MailingLists"].DefaultView;
			}
		}
		
		#endregion
		

		#region method AddMailingList

		/// <summary>
		/// Adds new mailing list.
		/// </summary>
		/// <param name="mailingListID">Mailing list ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="mailingListName">Mailing list name name. Eg. all@lumisoft.ee .</param>
		/// <param name="description">Mailing list description.</param>
		/// <param name="domainName">Domain name. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <param name="enabled">Specifies if mailing list is enabled.</param>
		/// <remarks>Throws exception if specified mailing list already exists.</remarks>
		public void AddMailingList(string mailingListID,string mailingListName,string description,string domainName,bool enabled)
		{
			if(mailingListID.Length == 0){
				throw new Exception("You must specify mailingListID");
			}
			if(mailingListName.Length == 0){
				throw new Exception("You must specify mailingListName");
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddMailingList")){
				sqlCmd.AddParameter("_mailingListID"   ,NpgsqlDbType.Varchar,mailingListID);
				sqlCmd.AddParameter("_mailingListName" ,NpgsqlDbType.Varchar,mailingListName);
				sqlCmd.AddParameter("_description"     ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_domainName"      ,NpgsqlDbType.Varchar,domainName);
				sqlCmd.AddParameter("_enabled"         ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method DeleteMailingList

		/// <summary>
		/// Deletes specified mailing list.
		/// </summary>
		/// <param name="mailingListID"> Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		/// <returns></returns>
		public void DeleteMailingList(string mailingListID)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteMailingList")){
				sqlCmd.AddParameter("_mailingListID" ,NpgsqlDbType.Varchar,mailingListID);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method UpdateMailingList

		/// <summary>
		/// Updates specified mailing list.
		/// </summary>
		/// <param name="mailingListID">Mailing list ID.</param>
		/// <param name="mailingListName">Mailing list name name. Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		/// <param name="description">Mailing list description.</param>
		/// <param name="domainName">Domain name. Use <see cref="IMailServerApi.GetDomains">>GetUsers()</see> to get valid values.</param>
		/// <param name="enabled">Specifies if mailing list is enabled.</param>
		public void UpdateMailingList(string mailingListID,string mailingListName,string description,string domainName,bool enabled)
		{
			if(mailingListName.Length == 0){
				throw new Exception("You must specify mailingListName");
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateMailingList")){
				sqlCmd.AddParameter("_mailingListID"   ,NpgsqlDbType.Varchar,mailingListID);
				sqlCmd.AddParameter("_mailingListName" ,NpgsqlDbType.Varchar,mailingListName);
				sqlCmd.AddParameter("_description"     ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_domainName"      ,NpgsqlDbType.Varchar,domainName);
				sqlCmd.AddParameter("_enabled"         ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method AddMailingListAddress

		/// <summary>
		/// Add new email address to specified mailing list.
		/// </summary>
		/// <param name="addressID">Address ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="mailingListName">Mailing list name name. Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		/// <param name="address">Mailing list member address.</param>
		/// <remarks>Throws exception if specified mailing list member already exists.</remarks>
		public void AddMailingListAddress(string addressID,string mailingListName,string address)
		{
			if(addressID.Length == 0){
				throw new Exception("You must specify addressID");
			}
			if(mailingListName.Length == 0){
				throw new Exception("You must specify mailingListName");
			}
			if(address.Length == 0){
				throw new Exception("You must specify address");
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddMailingListAddress")){
				sqlCmd.AddParameter("_addressID"       ,NpgsqlDbType.Varchar,addressID);
				sqlCmd.AddParameter("_mailingListName" ,NpgsqlDbType.Varchar,mailingListName);
				sqlCmd.AddParameter("_address"         ,NpgsqlDbType.Varchar,address);
							
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method DeleteMailingListAddress

		/// <summary>
		/// Deletes specified email address from mailing list. 
		/// </summary>
		/// <param name="addressID">Mailing list member address ID. Use <see cref="IMailServerApi.GetMailingListAddresses">GetMailingListMembers()</see> to get valid values.</param>
		public void DeleteMailingListAddress(string addressID)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteMailingListAddress")){
				sqlCmd.AddParameter("_addressID" ,NpgsqlDbType.Varchar,addressID);
							
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method GetMailingListAddresses

		/// <summary>
		/// Gets mailing list members.
		/// </summary>
		/// <param name="mailingListName">Mailing list name name. Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		public DataView GetMailingListAddresses(string mailingListName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMailingListAddresses")){
				if(mailingListName.Length > 0){
					sqlCmd.AddParameter("_mailingListName",NpgsqlDbType.Varchar,mailingListName);
				}

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "MailingListAddresses";

				return ds.Tables["MailingListAddresses"].DefaultView;
			}
		}

		#endregion


        #region method GetMailingListACL

        /// <summary>
        /// Gets mailing list ACL list.
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        public DataView GetMailingListACL(string mailingListName)
        {
            /* Implementation notes:
                *) Get mailing list ACL list.
            */

            // Get mailing list ACL list
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMailingListACL")){
                sqlCmd.AddParameter("_mailingListName" ,NpgsqlDbType.Varchar,mailingListName);

                DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "ACL";

                DataSet dsRetVal = new DataSet();
                dsRetVal.Tables.Add("ACL");
                dsRetVal.Tables["ACL"].Columns.Add("UserOrGroup");
                foreach(DataRow dr in ds.Tables[0].Rows){
                    DataRow drNew = dsRetVal.Tables["ACL"].NewRow();
                    drNew["UserOrGroup"] = dr[0].ToString();
                    dsRetVal.Tables["ACL"].Rows.Add(drNew);
                }

				return dsRetVal.Tables["ACL"].DefaultView;
			}
        }

        #endregion

        #region method AddMailingListACL

        /// <summary>
        /// Adds specified user or group to mailing list ACL list (specified user can send messages to the specified mailing list).
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        /// <param name="userOrGroup">User or group name.</param>
        public void AddMailingListACL(string mailingListName,string userOrGroup)
        {
            /* Implementation notes:
                *) Ensure that mailing list exists.
                *) Ensure that user or group already doesn't exist in list.
                *) Add ACL entry.
            */

            /* We handle these is SQL, sql returns these errors in ErrorText
             
                *) Ensure that mailing list exists.
                *) Ensure that user or group already doesn't exist in list.
             
            */

            // Add ACL entry
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddMailingListACL")){
				sqlCmd.AddParameter("_mailingListName" ,NpgsqlDbType.Varchar,mailingListName);
                sqlCmd.AddParameter("_userOrGroup"     ,NpgsqlDbType.Varchar,userOrGroup);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method DeleteMailingListACL

        /// <summary>
        /// Deletes specified user or group from mailing list ACL list.
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        /// <param name="userOrGroup">User or group name.</param>
        public void DeleteMailingListACL(string mailingListName,string userOrGroup)
        {
            /* Implementation notes:
                *) Ensure that mailing list exists.
                *) Delete ACL entry.
            */

            /* We handle these is SQL, sql returns these errors in ErrorText
             
                *) Ensure that mailing list exists.
             
            */

            // Delete group and it's emebres
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteMailingListACL")){
				sqlCmd.AddParameter("_mailingListName" ,NpgsqlDbType.Varchar,mailingListName);
                sqlCmd.AddParameter("_userOrGroup"     ,NpgsqlDbType.Varchar,userOrGroup);
							
				DataSet ds = sqlCmd.Execute();
                ds.Tables[0].TableName = "Result";

                // Proccess errors
				if(ds.Tables["Result"].Rows.Count > 0 && ds.Tables["Result"].Rows[0]["ErrorText"].ToString().Length > 0){
					throw new Exception(ds.Tables["Result"].Rows[0]["ErrorText"].ToString());
				}
			}
        }

        #endregion

        #region method CanAccessMailingList

        /// <summary>
        /// Checks if specified user can access specified mailing list.
        /// There is one built-in user anyone, that represent all users (including anonymous).
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        /// <param name="user">User name.</param>
        /// <returns></returns>
        public bool CanAccessMailingList(string mailingListName,string user)
        {
            /* Implementation notes:
                *) Ensure that mailing list exists.
                *) Check access.
            */

            // Ensure that mailing list exists
            // Get mailing list ID
            string mailingListID = null;
            foreach(DataRowView drV in GetMailingLists("ALL")){
                if(drV["MailingListName"].ToString().ToLower() == mailingListName.ToLower()){
                    mailingListID = drV["MailingListID"].ToString();
                    break;
                }
            }                
            if(mailingListID == null){
                throw new Exception("Invalid mailing list name, specified mailing list '" + mailingListName + "' doesn't exist !");
            }

            // Check access
            WSqlCommand cmd = new WSqlCommand(m_ConStr,"select * from lsMailingListACL");
            cmd.CommandType = CommandType.Text;
            DataSet dsMailingListACL = cmd.Execute();
            dsMailingListACL.Tables[0].TableName = "ACL";

            foreach(DataRow dr in dsMailingListACL.Tables["ACL"].Rows){
                if(dr["MailingListID"].ToString() == mailingListID){
                    // Built-in anyone
                    if(dr["UserOrGroup"].ToString().ToLower() == "anyone"){
                        return true;
                    }
                    // Built-in "authenticated users"
                    else if(dr["UserOrGroup"].ToString().ToLower() == "authenticated users"){
                        return UserExists(user);
                    }
                    // User or group
                    else{                            
                        if(GroupExists(dr["UserOrGroup"].ToString())){
                            return IsUserGroupMember(dr["UserOrGroup"].ToString(),user);
                        }                    
                        else{
                            return UserExists(user);
                        }
                    }
                }
            }

            return false;
        }

        #endregion


        #region method MailingListExists

        /// <summary>
		/// Checks if user exists.
		/// </summary>
		/// <param name="mailingListName">Mailing list name.</param>
		/// <returns>Returns true if mailing list exists.</returns>
		public bool MailingListExists(string mailingListName)
		{		
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMailingListProperties")){
				sqlCmd.AddParameter("_mailingListName",NpgsqlDbType.Varchar,mailingListName);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "MailingLists";

				if(ds.Tables["MailingLists"].Rows.Count > 0){
					return true;
				}
			}

			return false;
		}
		
		#endregion

		#endregion


        #region Rules

        #region method GetGlobalMessageRules

        /// <summary>
        /// Gets global message rules.
        /// </summary>
        /// <returns></returns>
        public DataView GetGlobalMessageRules()
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetGlobalMessageRules")){				
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "GlobalMessageRules";

                // We need to convert MatchExpression to string. 
                // MSSQL won't allow strings bigger than 400 chars, because of it we store it as Bytea.
                ds.Tables[0].Columns["MatchExpression"].ColumnName = "MatchExpressionOld";
                ds.Tables[0].Columns.Add("MatchExpression",typeof(string));
                foreach(DataRow dr in ds.Tables[0].Rows){
                    dr["MatchExpression"] = System.Text.Encoding.Default.GetString((byte[])dr["MatchExpressionOld"]);
                }
                ds.Tables[0].Columns.Remove("MatchExpressionOld");
                

				return ds.Tables["GlobalMessageRules"].DefaultView;
			}
        }

        #endregion

        #region method AddGlobalMessageRule

        /// <summary>
        /// Adds new global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void AddGlobalMessageRule(string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddGlobalMessageRule")){
				sqlCmd.AddParameter("_ruleID"          ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_cost"            ,NpgsqlDbType.Bigint ,cost);
				sqlCmd.AddParameter("_enabled"         ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_checkNextRule"   ,NpgsqlDbType.Integer,checkNextRule);
				sqlCmd.AddParameter("_description"     ,NpgsqlDbType.Varchar,description);
                sqlCmd.AddParameter("_matchExpression" ,NpgsqlDbType.Bytea  ,System.Text.Encoding.Default.GetBytes(matchExpression));
							
				DataSet ds = sqlCmd.Execute();
			}
        }
        
        #endregion

        #region method DeleteGlobalMessageRule

        /// <summary>
        /// Deletes specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID of rule which to delete.</param>
        public void DeleteGlobalMessageRule(string ruleID)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteGlobalMessageRule")){
				sqlCmd.AddParameter("_ruleID" ,NpgsqlDbType.Varchar,ruleID);
							
				DataSet ds = sqlCmd.Execute();
			}
        }
       
        #endregion

        #region method UpdateGlobalMessageRule

        /// <summary>
        /// Updates specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void UpdateGlobalMessageRule(string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateGlobalMessageRule")){
				sqlCmd.AddParameter("_ruleID"          ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_cost"            ,NpgsqlDbType.Bigint ,cost);
				sqlCmd.AddParameter("_enabled"         ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_checkNextRule"   ,NpgsqlDbType.Integer,checkNextRule);
				sqlCmd.AddParameter("_description"     ,NpgsqlDbType.Varchar,description);
                sqlCmd.AddParameter("_matchExpression" ,NpgsqlDbType.Bytea  ,System.Text.Encoding.Default.GetBytes(matchExpression));
							
				DataSet ds = sqlCmd.Execute();
			}
        }
        
        #endregion

        #region method GetGlobalMessageRuleActions

        /// <summary>
        /// Gets specified global message rule actions.
        /// </summary>
        /// <param name="ruleID">Rule ID of rule which actions to get.</param>
        public DataView GetGlobalMessageRuleActions(string ruleID)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetGlobalMessageRuleActions")){
			    sqlCmd.AddParameter("_ruleID",NpgsqlDbType.Varchar,ruleID);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "GlobalMessageRuleActions";

				return ds.Tables["GlobalMessageRuleActions"].DefaultView;
			}
        }

        #endregion

        #region method AddGlobalMessageRuleAction

        /// <summary>
        /// Adds action to specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID to which to add this action.</param>
        /// <param name="actionID">Action ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void AddGlobalMessageRuleAction(string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddGlobalMessageRuleAction")){
				sqlCmd.AddParameter("_ruleID"      ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_actionID"    ,NpgsqlDbType.Varchar,actionID);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_actionType"  ,NpgsqlDbType.Integer,(int)actionType);
                sqlCmd.AddParameter("_actionData"  ,NpgsqlDbType.Bytea  ,actionData);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method DeleteGlobalMessageRuleAction

        /// <summary>
        /// Deletes specified action from specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID which action to delete.</param>
        /// <param name="actionID">Action ID of action which to delete.</param>
        public void DeleteGlobalMessageRuleAction(string ruleID,string actionID)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteGlobalMessageRuleAction")){
				sqlCmd.AddParameter("_ruleID"   ,NpgsqlDbType.Varchar,ruleID);
                sqlCmd.AddParameter("_actionID" ,NpgsqlDbType.Varchar,actionID);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method UpdateGlobalMessageRuleAction

        /// <summary>
        /// Updates specified rule action.
        /// </summary>
        /// <param name="ruleID">Rule ID which action to update.</param>
        /// <param name="actionID">Action ID of action which to update.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void UpdateGlobalMessageRuleAction(string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateGlobalMessageRuleAction")){
				sqlCmd.AddParameter("_ruleID"      ,NpgsqlDbType.Varchar,ruleID);
				sqlCmd.AddParameter("_actionID"    ,NpgsqlDbType.Varchar,actionID);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_actionType"  ,NpgsqlDbType.Integer,(int)actionType);
                sqlCmd.AddParameter("_actionData"  ,NpgsqlDbType.Bytea  ,actionData);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #endregion


		#region Routing related

		#region method GetRoutes

		/// <summary>
		/// Gets email address routes.
		/// </summary>
		/// <returns></returns>
		public DataView GetRoutes()
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetRoutes")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Routing";

				return ds.Tables["Routing"].DefaultView;
			}
		}
		
		#endregion


		#region method AddRoute

		/// <summary>
		/// Adds new route.
		/// </summary>
		/// <param name="routeID">Route ID.</param>
        /// <param name="cost">Cost specifies in what order roues are processed. Costs with lower values are processed first.</param>
		/// <param name="enabled">Specifies if route is enabled.</param>
		/// <param name="description">Route description text.</param>
		/// <param name="pattern">Match pattern. For example: *,*@domain.com,*sales@domain.com.</param>
		/// <param name="action">Specifies route action.</param>
		/// <param name="actionData">Route action data.</param>
		public void AddRoute(string routeID,long cost,bool enabled,string description,string pattern,RouteAction_enum action,byte[] actionData)
		{
			if(routeID.Length == 0){
				throw new Exception("You must specify routeID");
			}
			if(pattern.Length == 0){
				throw new Exception("You must specify pattern");
			}
            
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddRoute")){
				sqlCmd.AddParameter("_routeID"     ,NpgsqlDbType.Varchar,routeID);
				sqlCmd.AddParameter("_cost"        ,NpgsqlDbType.Bigint ,cost);
				sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_pattern"     ,NpgsqlDbType.Varchar,pattern);
                sqlCmd.AddParameter("_action"      ,NpgsqlDbType.Integer,action);
                sqlCmd.AddParameter("_actionData"  ,NpgsqlDbType.Bytea  ,actionData);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method DeleteRoute

		/// <summary>
		/// Deletes route.
		/// </summary>
		/// <param name="routeID">Route ID.</param>
		public void DeleteRoute(string routeID)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteRoute")){
				sqlCmd.AddParameter("_routeID" ,NpgsqlDbType.Varchar,routeID);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method UpdateRoute

		/// <summary>
		/// Updates route.
		/// </summary>
		/// <param name="routeID">Route ID.</param>
        /// <param name="cost">Cost specifies in what order roues are processed. Costs with lower values are processed first.</param>
		/// <param name="enabled">Specifies if route is enabled.</param>
		/// <param name="description">Route description text.</param>
		/// <param name="pattern">Match pattern. For example: *,*@domain.com,*sales@domain.com.</param>
		/// <param name="action">Specifies route action.</param>
		/// <param name="actionData">Route action data.</param>
		public void UpdateRoute(string routeID,long cost,bool enabled,string description,string pattern,RouteAction_enum action,byte[] actionData)
		{
			if(pattern.Length == 0){
				throw new Exception("You must specify pattern");
			}
            
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateRoute")){
				sqlCmd.AddParameter("@routeID"     ,NpgsqlDbType.Varchar,routeID);
				sqlCmd.AddParameter("@cost"        ,NpgsqlDbType.Bigint ,cost);
				sqlCmd.AddParameter("@enabled"     ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("@description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("@pattern"     ,NpgsqlDbType.Varchar,pattern);
                sqlCmd.AddParameter("@action"      ,NpgsqlDbType.Integer,action);
                sqlCmd.AddParameter("@actionData"  ,NpgsqlDbType.Bytea  ,actionData);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#endregion


		#region MailStore related

		#region method GetMessagesInfo

		/// <summary>
        /// Gets specified IMAP folder messages info. 
        /// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what messages info to get. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="messages">IMAP_Messages collection where to store folder messages info.</param>
        public void GetMessagesInfo(string accessingUser,string folderOwnerUser,string folder,IMAP_MessageCollection messages)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'r' permission.
                    There is builtin user system, skip ACL for it.
                *) Fill messages info.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(messages);
            //---------------------------------------//
            			
            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'r' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.r) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Fill messages info
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMessagesInfo")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folder"  ,NpgsqlDbType.Varchar,folder);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "lsMailStore";

				foreach(DataRow dr in ds.Tables["lsMailStore"].Rows){
					string   messageID = dr["MessageID"].ToString();
					int      size      = Convert.ToInt32(dr["Size"]);
					DateTime date      = Convert.ToDateTime(dr["Date"]); 
					int      flags     = Convert.ToInt32(dr["MessageFlags"]);
					int      uid       = Convert.ToInt32(dr["UID"]);

					messages.Add(
                        messageID,
                        uid,
                        date,
                        size,
                        (IMAP_MessageFlags)flags
                    );
				}
			}
		}
		
		#endregion


		#region method StoreMessage

		/// <summary>
		/// Stores message to specified folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder where to store message. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="msgStream">Stream where message has stored. Stream position must be at the beginning of the message.</param>
		/// <param name="date">Recieve date.</param>
		/// <param name="flags">Message flags.</param>
		public void StoreMessage(string accessingUser,string folderOwnerUser,string folder,Stream msgStream,DateTime date,IMAP_MessageFlags flags)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'p' or 'i' permission.
                    There is builtin user system, skip ACL for it.
                *) Store message.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(msgStream);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'p' or 'i' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.p) == 0 && (acl & IMAP_ACL_Flags.i) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Store message
            MemoryStream msgMemStream = new MemoryStream();
            Net_Utils.StreamCopy(msgStream,msgMemStream,32000);
	        byte[] messageData = msgMemStream.ToArray();
            byte[] topLines = GetTopLines(new MemoryStream(messageData),50);
            Mail_Message message = null;
            try{
                msgStream.Position = 0;
                message = Mail_Message.ParseFromByte(messageData);
            }
            catch(Exception x){
                message = new Mail_Message();
                message.MimeVersion = "1.0";
                message.MessageID = MIME_Utils.CreateMessageID();
                message.Date = DateTime.Now;
                message.From = new Mail_t_MailboxList();
                message.From.Add(new Mail_t_Mailbox("system","system"));
                message.To = new Mail_t_AddressList();
                message.To.Add(new Mail_t_Mailbox("system","system"));
                message.Subject = "[BAD MESSAGE] Bad message, message parsing failed !";

                //--- multipart/mixed -------------------------------------------------------------------------------------------------
                MIME_h_ContentType contentType_multipartMixed = new MIME_h_ContentType(MIME_MediaTypes.Multipart.mixed);
                contentType_multipartMixed.Param_Boundary = Guid.NewGuid().ToString().Replace('-','.');
                MIME_b_MultipartMixed multipartMixed = new MIME_b_MultipartMixed(contentType_multipartMixed);
                message.Body = multipartMixed;

                    //--- text/plain ---------------------------------------------------------------------------------------------------
                    MIME_Entity entity_text_plain = new MIME_Entity();
                    MIME_b_Text text_plain = new MIME_b_Text(MIME_MediaTypes.Text.plain);
                    entity_text_plain.Body = text_plain;
                    text_plain.SetText(MIME_TransferEncodings.QuotedPrintable,Encoding.UTF8,"NOTE: Bad message, message parsing failed.\r\n\r\n");
                    multipartMixed.BodyParts.Add(entity_text_plain);
            }
            byte[] envelope = System.Text.Encoding.Default.GetBytes(IMAP_Envelope.ConstructEnvelope(message));
            byte[] body     = System.Text.Encoding.Default.GetBytes(IMAP_BODY.ConstructBodyStructure(message,false));

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_StoreMessage")){
				sqlCmd.AddParameter("_userName"     ,NpgsqlDbType.Varchar   ,folderOwnerUser);
				sqlCmd.AddParameter("_folder"       ,NpgsqlDbType.Varchar   ,folder);
				sqlCmd.AddParameter("_messageID"    ,NpgsqlDbType.Varchar   ,Guid.NewGuid().ToString());
				sqlCmd.AddParameter("_size"         ,NpgsqlDbType.Integer   ,messageData.Length);
				sqlCmd.AddParameter("_messageFlags" ,NpgsqlDbType.Integer   ,(int)flags);
				sqlCmd.AddParameter("_date"         ,NpgsqlDbType.Timestamp ,date);
				sqlCmd.AddParameter("_topLines"     ,NpgsqlDbType.Bytea     ,topLines);
				sqlCmd.AddParameter("_data"         ,NpgsqlDbType.Bytea     ,messageData);
                sqlCmd.AddParameter("_imapEnvelope" ,NpgsqlDbType.Bytea     ,envelope);
                sqlCmd.AddParameter("_imapBody"     ,NpgsqlDbType.Bytea     ,body);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method StoreMessageFlags

		/// <summary>
		/// Stores IMAP message flags (\seen,\draft, ...).
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder which message flags to store. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="message">Fix ME: ???</param>
		/// <param name="msgFlags">Message flags to store.</param>
		public void StoreMessageFlags(string accessingUser,string folderOwnerUser,string folder,LumiSoft.Net.IMAP.Server.IMAP_Message message,IMAP_MessageFlags msgFlags)
		{
           /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) Remove all message flags which permissions user doesn't have.
                *) Store message.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(message);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // Remove all message flags which permissions user doesn't have.
            if(accessingUser != "system"){
			    IMAP_ACL_Flags userACL = GetUserACL(folderOwnerUser,folder,accessingUser);					
			    if((userACL & IMAP_ACL_Flags.s) == 0){
    				msgFlags &= ~IMAP_MessageFlags.Seen;
			    }
			    else if((userACL & IMAP_ACL_Flags.d) == 0){
				    msgFlags &= ~IMAP_MessageFlags.Deleted;
			    }				
			    else if((userACL & IMAP_ACL_Flags.s) == 0){
				    msgFlags &= (~IMAP_MessageFlags.Answered | ~IMAP_MessageFlags.Draft | ~IMAP_MessageFlags.Flagged);
			    }
            }

            //--- Store message flags
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_StoreMessageFlags")){
				sqlCmd.AddParameter("_userName"     ,NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folder"       ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_messageID"    ,NpgsqlDbType.Varchar,message.ID);
				sqlCmd.AddParameter("_messageFlags" ,NpgsqlDbType.Integer,(int)message.Flags);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method DeleteMessage

		/// <summary>
		/// Deletes message from mailbox.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what message to delete. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="messageID">Message ID.</param>
		/// <param name="uid">Message UID value.</param>
		public void DeleteMessage(string accessingUser,string folderOwnerUser,string folder,string messageID,int uid)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'd' permission.
                    There is builtin user system, skip ACL for it.
                *) Fill messages info.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(messageID);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'd' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.d) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Recycle bin handling ----------------------------------------------------------------------//
            if(Convert.ToBoolean(GetRecycleBinSettings().Rows[0]["DeleteToRecycleBin"])){
                EmailMessageItems msgItems = new EmailMessageItems(messageID,IMAP_MessageItems_enum.Message | IMAP_MessageItems_enum.Envelope);
                GetMessageItems("system",folderOwnerUser,folder,msgItems);
                if(msgItems.MessageExists){                    
                    byte[] data = new byte[msgItems.MessageStream.Length];
                    msgItems.MessageStream.Read(data,0,data.Length);

                    using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_StoreRecycleBinMessage")){
				        sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,Guid.NewGuid().ToString());
                        sqlCmd.AddParameter("_user"      ,NpgsqlDbType.Varchar,folderOwnerUser);
                        sqlCmd.AddParameter("_folder"    ,NpgsqlDbType.Varchar,folder);
                        sqlCmd.AddParameter("_size"      ,NpgsqlDbType.Bigint ,data.Length);
                        sqlCmd.AddParameter("_envelope"  ,NpgsqlDbType.Varchar,msgItems.Envelope);
                        sqlCmd.AddParameter("_data"      ,NpgsqlDbType.Bytea  ,data);

        				DataSet ds = sqlCmd.Execute();                        
			        }
                }
            }
            //----------------------------------------------------------------------------------------------//

            //--- Delete message
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteMessage")){
                sqlCmd.AddParameter("_userName"  ,NpgsqlDbType.Varchar,folderOwnerUser);				
				sqlCmd.AddParameter("_folder"    ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,messageID);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

        #region method GetMessageItems

        /// <summary>
        /// Gets specified message specified items.
        /// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what message to delete. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="e">MessageItems info.</param>
        public void GetMessageItems(string accessingUser,string folderOwnerUser,string folder,EmailMessageItems e)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'r' permission.
                    There is builtin user system, skip ACL for it.
                *) Store message.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(e);
            //---------------------------------------//
            
            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'r' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.r) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Get message
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMessagesItems")){
				sqlCmd.AddParameter("_userName"  ,NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folder"    ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,e.MessageID);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "lsMailStore";
                
                if((e.MessageItems & IMAP_MessageItems_enum.BodyStructure) != 0){
                    e.BodyStructure = System.Text.Encoding.Default.GetString((byte[])ds.Tables["lsMailStore"].Rows[0]["ImapBody"]).Substring(5);
                }
                if((e.MessageItems & IMAP_MessageItems_enum.Envelope) != 0){
                    e.Envelope = System.Text.Encoding.Default.GetString((byte[])ds.Tables["lsMailStore"].Rows[0]["ImapEnvelope"]);
                }
                if((e.MessageItems & IMAP_MessageItems_enum.Header) != 0){
                    e.Header = GetTopLines(new MemoryStream((byte[])ds.Tables["lsMailStore"].Rows[0]["TopLines"]),0);
                }
                if((e.MessageItems & IMAP_MessageItems_enum.Message) != 0){
                    using(WSqlCommand sqlCmd2 = new WSqlCommand(m_ConStr,"lspr_GetMessage")){
				        sqlCmd2.AddParameter("_userName"  ,NpgsqlDbType.Varchar,folderOwnerUser);
				        sqlCmd2.AddParameter("_folder"    ,NpgsqlDbType.Varchar,folder);
				        sqlCmd2.AddParameter("_messageID" ,NpgsqlDbType.Varchar,e.MessageID);

				        DataSet ds2 = sqlCmd2.Execute();
				        ds2.Tables[0].TableName = "lsMailStore";
                   
                        e.MessageStream = new MemoryStream((byte[])ds2.Tables["lsMailStore"].Rows[0]["Data"]);
                    }
                }
			}
        }

        #endregion

		#region method GetMessageTopLines

		/// <summary>
		/// Gets message header + number of specified lines.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what message top lines to get. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="msgID">MessageID.</param>
		/// <param name="nrLines">Number of lines to retrieve. NOTE: line counting starts at the end of header.</param>
		/// <returns>Returns message header + number of specified lines.</returns>
		public byte[] GetMessageTopLines(string accessingUser,string folderOwnerUser,string folder,string msgID,int nrLines)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'r' permission.
                    There is builtin user system, skip ACL for it.
                *) Get message top lines.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(msgID);
            //---------------------------------------//
            
            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'r' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.r) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Get message top lines
			if(nrLines < 50){
				using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMessageTopLines")){					
					sqlCmd.AddParameter("_userName"  ,NpgsqlDbType.Varchar,folderOwnerUser);
					sqlCmd.AddParameter("_folder"    ,NpgsqlDbType.Varchar,folder);
                    sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,msgID);

                    return GetTopLines(new MemoryStream((byte[])sqlCmd.ExecuteScalar()),nrLines);
				}
			}
			else{
                EmailMessageItems msgItems = new EmailMessageItems(msgID,IMAP_MessageItems_enum.Message);            
                GetMessageItems(accessingUser,folderOwnerUser,folder,msgItems);
                byte[] topLines = GetTopLines(msgItems.MessageStream,nrLines);
                msgItems.MessageStream.Dispose();
				return topLines;
			}
		}
		
		#endregion

		#region method CopyMessage

		/// <summary>
		/// Creates copy of message to destination IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what contains message to copy. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="destFolderUser">Destination IMAP folder owner user name.</param>
		/// <param name="destFolder">Destination IMAP folder name.</param>
		/// <param name="message">IMAP message which to copy.</param>
		public void CopyMessage(string accessingUser,string folderOwnerUser,string folder,string destFolderUser,string destFolder,LumiSoft.Net.IMAP.Server.IMAP_Message message)
		{
           /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) We don't need to map shared folder, check security, it done by GetMessage and StoreMessage methods.
                *) Copy message.               
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateUserName(destFolderUser);
            ArgsValidator.ValidateFolder(destFolder);
            ArgsValidator.ValidateNotNull(message);
            //---------------------------------------//
            
            //--- Copy message
            //--- Copy message
            EmailMessageItems msgItems = new EmailMessageItems(message.ID,IMAP_MessageItems_enum.Message);            
            GetMessageItems(accessingUser,folderOwnerUser,folder,msgItems);
            StoreMessage("system",destFolderUser,destFolder,msgItems.MessageStream,message.InternalDate,message.Flags);
            msgItems.MessageStream.Dispose();
		}
		
		#endregion


		#region method GetFolders
        		
        /// <summary>
		/// Gets all available IMAP folders.
		/// </summary>
		/// <param name="userName">User name who's folders to get.</param>
        /// <param name="includeSharedFolders">If true, shared folders are included.</param>
		public string[] GetFolders(string userName,bool includeSharedFolders)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'l' permission.                             
                *) Append all visible(Forlders on what user has 'r' right) user mailbox folders.
                *) Append all visible(Forlders on what user has 'r' right) public folders to folders list !
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(userName);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(userName)){
                throw new Exception("User '" + userName + "' desn't exist !");
            }

            DataSet dsFolders = null;
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetFolders")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				dsFolders = sqlCmd.Execute();
				dsFolders.Tables[0].TableName = "Folders";
			}

            // Append all visible(Forlders on what user has 'r' right) user mailbox folders.
            List<string> userFolders = new List<string>();
			for(int i=0;i<dsFolders.Tables["Folders"].Rows.Count;i++){
                string folder = dsFolders.Tables["Folders"].Rows[i]["FolderName"].ToString();
                // Show folders what user has 'r' right
                if((GetUserACL(userName,folder,userName) & IMAP_ACL_Flags.r) != 0){
                    userFolders.Add(folder);
                }
			}
            
            // Append all visible(Forlders on what user has 'r' right) shared folders to folders list
            if(includeSharedFolders){
                SharedFolderRoot[] sharedFolderRoots = GetSharedFolderRoots();
                foreach(SharedFolderRoot sharedFolderRoot in sharedFolderRoots){
                    // Skip disabled roots                    
                    if(!sharedFolderRoot.Enabled){
                        continue;
                    }

                    // Root is bounded folder
                    if(sharedFolderRoot.RootType == SharedFolderRootType_enum.BoundedRootFolder){
                        // Get root child folders
                        string[] boundedFolders = GetFolders(sharedFolderRoot.BoundedUser,false);
                        foreach(string boundedFolder in boundedFolders){
                            // We want folders what will be BoundedFolder child folders, others just skip
                            if(boundedFolder.ToLower().StartsWith(sharedFolderRoot.BoundedFolder.ToLower())){
                                // Show only folder what user has 'lr' right
                                if((GetUserACL(sharedFolderRoot.BoundedUser,boundedFolder,userName) & IMAP_ACL_Flags.r) != 0){
                                    // We must cut of boundedFolder part. For example root name = 'Public'and 
                                    // boundedFolder = 'inbox', then it must be visible as Public/inboxChildFolders,
                                    // not Public/inbox/inboxChildFolders.
                                    userFolders.Add(sharedFolderRoot.FolderName + boundedFolder.Substring(sharedFolderRoot.BoundedFolder.Length));
                                }
                            }
                        }
                    }
                    // Root is Users Shared Folders namespace
                    else{
                        // Get shared user on which accessing user has access rights
                        List<string> sharingUsers = new List<string>();
                        foreach(DataRow dr in GetACL().Tables["ACL"].Rows){
                            string userOrGroup = dr["User"].ToString().ToLower();
                            string sharingUserName = dr["Folder"].ToString().Split(new char[]{'/'},2)[0];

                            // anyone access, so we have access to that folder
                            if(userOrGroup == "anyone"){
                                if(!sharingUsers.Contains(sharingUserName)){
                                    sharingUsers.Add(sharingUserName);
                                }
                            }
                            // accessing user has access to that folder
                            else if(userOrGroup == userName.ToLower()){
                                if(!sharingUsers.Contains(sharingUserName)){
                                    sharingUsers.Add(sharingUserName);
                                }
                            }
                            else{
                                // If group ACL set, see if accessing user is member of that group
                                DataRow drGroup = GetGroup(userOrGroup);
                                if(drGroup != null && IsUserGroupMember(drGroup["GroupName"].ToString(),userName)){
                                    if(!sharingUsers.Contains(sharingUserName)){
                                        sharingUsers.Add(sharingUserName);
                                    }
                                }                                
                            }
                        }

                        //--- Show folders on what user has enough ACL
                        foreach(string sharingUser in sharingUsers){
                            string[] sharingUserFolders = GetFolders(sharingUser,false);
                            foreach(string sharingUserFolder in sharingUserFolders){
                                // Show only folder what user has 'lr' right
                                if((GetUserACL(sharingUser,sharingUserFolder,userName) & IMAP_ACL_Flags.r) != 0){                                
                                    userFolders.Add(sharedFolderRoot.FolderName + "/" + sharingUser + "/" + sharingUserFolder);
                                }
                            }
                        }
                    }                   
                }
            }

			return userFolders.ToArray();
        }
		
		#endregion

		#region method GetSubscribedFolders

		/// <summary>
		/// Gets subscribed IMAP folders.
		/// </summary>
		/// <param name="userName"></param>
		public string[] GetSubscribedFolders(string userName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetSubscribedFolders")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "Folders";

				string[] retVal = new string[ds.Tables["Folders"].Rows.Count];
				int i = 0;
				foreach(DataRow dr in ds.Tables["Folders"].Rows){
					retVal[i] = dr["FolderName"].ToString();
					i++;
				}

				return retVal;
			}
		}
		
		#endregion

		#region method SubscribeFolder

		/// <summary>
		/// Subscribes new IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void SubscribeFolder(string userName,string folder)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_SubscribeFolder")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);
				sqlCmd.AddParameter("_folder"  ,NpgsqlDbType.Varchar,folder);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method UnSubscribeFolder

		/// <summary>
		/// UnSubscribes IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void UnSubscribeFolder(string userName,string folder)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UnSubscribeFolder")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);
				sqlCmd.AddParameter("_folder"  ,NpgsqlDbType.Varchar,folder);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method CreateFolder

		/// <summary>
		/// Creates new IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what contains message to copy. For example: Inbox,Public Folders/Documnets .</param>
		public void CreateFolder(string accessingUser,string folderOwnerUser,string folder)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder doesn't exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'c' permission.
                    There is builtin user system, skip ACL for it.
                *) Create folder.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder doesn't exists. Throw Exception if don't.
            if(FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' already exist !");
            }

            // See if user has sufficient permissions. User requires 'c' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.c) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Create folder
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_CreateFolder")){
                sqlCmd.AddParameter("_folderID",NpgsqlDbType.Varchar,Guid.NewGuid().ToString());
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folder"  ,NpgsqlDbType.Varchar,folder);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method DeleteFolder

		/// <summary>
		/// Deletes IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what to delete. For example: Inbox,Public Folders/Documnets .</param>
		public void DeleteFolder(string accessingUser,string folderOwnerUser,string folder)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Don't allow to delete shared folders root folder.
                   For BoundedUser root don't allow root folder only,
                   for UsersShared root don't allow root + user name.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'c' permission.
                   There is builtin user system, skip ACL for it.
                *) Create folder.
            */

			// Don't allow to delete inbox
			if(folder.ToLower() == "inbox"){
			    throw new Exception("Can't delete inbox");
		    }

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                /* Don't allow to delete shared folders root folder.
                   For BoundedUser root don't allow root folder only,
                   for UsersShared root don't allow root + user name.
                */

                // Main shared folder root.
                if(mappedFolder.SharedRootName.ToLower() == originalFolder.ToLower()){
                    throw new ArgumentException("Can't delete shared root folder '" + originalFolder + "' !");
                }
                // Users shared folder: root/username -> no folder
                if(folder == ""){
                    throw new ArgumentException("Can't delete shared root folder '" + originalFolder + "' !");
                }

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder doesn't exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'c' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.c) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }


            //--- Recycle bin handling ----------------------------------------------------------------------//
            if(Convert.ToBoolean(GetRecycleBinSettings().Rows[0]["DeleteToRecycleBin"])){
                IMAP_MessageCollection messages = new IMAP_MessageCollection();
                GetMessagesInfo("system",folderOwnerUser,folder,messages);
                foreach(IMAP_Message message in messages){
                    EmailMessageItems msgItems = new EmailMessageItems(message.ID,IMAP_MessageItems_enum.Message);
                    GetMessageItems("system",folderOwnerUser,folder,msgItems);
                    if(msgItems.MessageExists){
                        string subject = "<none>";
                        try{
                            subject = MIME_Utils.ParseHeaderField("Subject:",msgItems.MessageStream);
                            subject = subject.Replace("\r","");
                            subject = subject.Replace("\n","");                        
                        }
                        catch{
                        }
                        msgItems.MessageStream.Position = 0;
                        byte[] data = new byte[msgItems.MessageStream.Length];
                        msgItems.MessageStream.Read(data,0,data.Length);

                        using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_StoreRecycleBinMessage")){
				            sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,Guid.NewGuid().ToString());
                            sqlCmd.AddParameter("_user"      ,NpgsqlDbType.Varchar,folderOwnerUser);
                            sqlCmd.AddParameter("_folder"    ,NpgsqlDbType.Varchar,folder);
                            sqlCmd.AddParameter("_subject"   ,NpgsqlDbType.Varchar,subject);
                            sqlCmd.AddParameter("_data"      ,NpgsqlDbType.Bytea  ,data);

        				    DataSet ds = sqlCmd.Execute();                        
			            }
                    }
                }
            }
            //----------------------------------------------------------------------------------------------//

            //--- Delete folder
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteFolder")){
				sqlCmd.AddParameter("_folderOwnerUser",NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folderName"     ,NpgsqlDbType.Varchar,folder);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method RenameFolder

		/// <summary>
		/// Renames IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what to delete. For example: Trash,Public Folders/Documnets .</param>
		/// <param name="newFolder">New folder name.</param>
		public void RenameFolder(string accessingUser,string folderOwnerUser,string folder,string newFolder)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folders. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Don't allow to rename shared folders root folder.
                   For BoundedUser root don't allow root folder only,
                   for UsersShared root don't allow root + user name.
                *) Ensure that source folder exists. Throw Exception if don't.
                *) Ensure that destinaton folder doesn't exists. Throw Exception if does.
                *) See if user has sufficient permissions. User requires 'c' permission.
                   There is builtin user system, skip ACL for it.
                *) Rename folder.
            */

		    // Don't allow to rename inbox
			if(folder.ToLower() == "inbox"){
			    throw new Exception("Can't rename inbox");
			}

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateFolder(newFolder);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;
        
                if(originalFolder.ToLower() == mappedFolder.SharedRootName.ToLower() || folderOwnerUser == "" || folder == ""){
                    throw new Exception("Can't rename shared root folder '" + originalFolder + "' !");
                }
            }

            // Normalize folder. Remove '/' from folder start and end.
            newFolder = API_Utlis.NormalizeFolder(newFolder);

            // Do Shared Folders mapping.
            string originalNewFolder = newFolder;
            string destinationFolderOwner = folderOwnerUser;
            SharedFolderMapInfo mappedNewFolder = MapSharedFolder(originalNewFolder);
            if(mappedNewFolder.IsSharedFolder){
                destinationFolderOwner = mappedNewFolder.FolderOnwer;
                newFolder = mappedNewFolder.Folder;

                if(originalNewFolder.ToLower() == mappedNewFolder.SharedRootName.ToLower() || destinationFolderOwner == "" || newFolder == ""){
                    throw new Exception("Invalid destination folder value, folder '" + originalNewFolder + "' alreay exists !");
                }
            }

            // Don't allow shared folder to change root folder
            // For example: "Public Folder/aaa" can't be renamed to "Public Folder New/aaa"
            if(mappedFolder.SharedRootName.ToLower() != mappedNewFolder.SharedRootName.ToLower() || mappedFolder.FolderOnwer.ToLower() != mappedNewFolder.FolderOnwer.ToLower()){
                throw new ArgumentException("Shared folder can't change root folder !");
            }
                                
            // Ensure that folder does exist. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // Ensure that folder doesn't exists. Throw Exception if does.
            if(FolderExists(folderOwnerUser + "/" + newFolder)){
                throw new Exception("Folder '" + newFolder + "' already exist !");
            }

            // See if user has sufficient permissions. User requires 'c' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.c) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Rename folder
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_RenameFolder")){
				sqlCmd.AddParameter("_userName"  ,NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folder"    ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_newFolder" ,NpgsqlDbType.Varchar,newFolder);

				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion
//**
		#region method FolderExists

		/// <summary>
		/// Gets if specified folder exists.
		/// </summary>
		/// <param name="folderName">Folder name which to check. Eg. UserName/Inbox,UserName/Inbox/subfolder.</param>
		/// <returns>Returns true if folder exists, otherwise false.</returns>
		public bool FolderExists(string folderName)
		{
			string[] user_folder = folderName.Split(new char[]{'/'},2);
			string   userName    = user_folder[0];
			if(user_folder.Length == 2){
				folderName = user_folder[1];
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_FolderExists")){                
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);
				sqlCmd.AddParameter("_folderName",NpgsqlDbType.Varchar,folderName);

				DataSet ds = sqlCmd.Execute();
				if(ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0){
					return true;
				}
			}

			return false;
		}

		#endregion

        #region method FolderCreationTime

        /// <summary>
		/// Gets time when specified folder was created.
		/// </summary>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what creation time to get. For example: Inbox,Public Folders/Documnets .</param>
		public DateTime FolderCreationTime(string folderOwnerUser,string folder)
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_FolderExists")){                
				sqlCmd.AddParameter("_userName"  ,NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folderName",NpgsqlDbType.Varchar,folder);

				DataSet ds = sqlCmd.Execute();
				if(ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0){
					return (DateTime)ds.Tables[0].Rows[0]["CreationTime"];
				}
                else{
                    throw new Exception("Folder '" + folderOwnerUser + "/" + folder + "' doesn't exist !");
                }
			}
        }

		#endregion


        #region method GetSharedFolderRoots

        /// <summary>
        /// Gets shared folder root folders.
        /// </summary>
        /// <returns></returns>
        public SharedFolderRoot[] GetSharedFolderRoots()
        {
            /* Implementation notes:
                *) Get shared folder roots.
            */

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetSharedFolderRoots")){
				DataSet dsSharedFolderRoots = sqlCmd.Execute();
				dsSharedFolderRoots.Tables[0].TableName = "SharedFoldersRoots";

                List<SharedFolderRoot> roots = new List<SharedFolderRoot>();
			    foreach(DataRow dr in dsSharedFolderRoots.Tables["SharedFoldersRoots"].Rows){
                    roots.Add(new SharedFolderRoot(
                        dr["RootID"].ToString(),
                        Convert.ToBoolean(dr["Enabled"]),
                        dr["Folder"].ToString(),
                        dr["Description"].ToString(),
                        (SharedFolderRootType_enum)Convert.ToInt32(dr["RootType"]),
                        dr["BoundedUser"].ToString(),
                        dr["BoundedFolder"].ToString()
                    ));
                }

                return roots.ToArray();
			}
        }

        #endregion

        #region method AddSharedFolderRoot

        /// <summary>
        /// Add shared folder root.
        /// </summary>
        /// <param name="rootID">Root folder ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="enabled">Specifies if root folder is enabled.</param>
        /// <param name="folder">Folder name which will be visible to public.</param>
        /// <param name="description">Description text.</param>
        /// <param name="rootType">Specifies what type root folder is.</param>
        /// <param name="boundedUser">User which to bound root folder.</param>
        /// <param name="boundedFolder">Folder which to bound to public folder.</param>
        public void AddSharedFolderRoot(string rootID,bool enabled,string folder,string description,SharedFolderRootType_enum rootType,string boundedUser,string boundedFolder)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that root ID doesn't exists.
                *) Ensure that root doesn't exists.
                *) Add root folder.
            */

            if(rootID == null || rootID == ""){
                throw new Exception("Invalid rootID value, rootID can't be '' or null !");
            }

            //--- Validate values -------------------------------------//
            ArgsValidator.ValidateNotNull(rootID);
            ArgsValidator.ValidateSharedFolderRoot(folder);
            ArgsValidator.ValidateNotNull(description);
            if(rootType == SharedFolderRootType_enum.BoundedRootFolder){
                ArgsValidator.ValidateUserName(boundedUser);
                ArgsValidator.ValidateFolder(boundedFolder);
            }
            //---------------------------------------------------------//

            /* We handle these is SQL, sql returns these errors in ErrorText
             
                *) Ensure that root ID doesn't exists.
                *) Ensure that root doesn't exists.             
            */

            // Insert group
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddSharedFolderRoot")){
				sqlCmd.AddParameter("_rootID"        ,NpgsqlDbType.Varchar,rootID);
				sqlCmd.AddParameter("_enabled"       ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("-folder"        ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_description"   ,NpgsqlDbType.Varchar,description);
                sqlCmd.AddParameter("_rootType"      ,NpgsqlDbType.Integer,rootType);
                sqlCmd.AddParameter("_boundedUser"   ,NpgsqlDbType.Varchar,boundedUser);
                sqlCmd.AddParameter("_boundedFolder" ,NpgsqlDbType.Varchar,boundedFolder);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method DeleteSharedFolderRoot

        /// <summary>
        /// Deletes shard folders root folder.
        /// </summary>
        /// <param name="rootID">Root folder ID which to delete.</param>
        public void DeleteSharedFolderRoot(string rootID)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that root ID exists.
                *) Delete root folder.
            */

            //--- Validate values -------------------------------------//
            if(rootID == null || rootID == ""){
                throw new Exception("Invalid rootID value, rootID can't be '' or null !");
            }
            //---------------------------------------------------------//

            /* We handle these is SQL, sql returns these errors in ErrorText
             
                *) Ensure that root ID exists.
                *) Delete root folder.
             
            */

            // Delete group and it's emebres
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteSharedFolderRoot")){
				sqlCmd.AddParameter("_rootID" ,NpgsqlDbType.Varchar,rootID);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method UpdateSharedFolderRoot

        /// <summary>
        /// Updates shared folder root.
        /// </summary>
        /// <param name="rootID">Root Folder IF which to update.</param>
        /// <param name="enabled">Specifies if root folder is enabled.</param>
        /// <param name="folder">Folder name which will be visible to public.</param>
        /// <param name="description">Description text.</param>
        /// <param name="rootType">Specifies what type root folder is.</param>
        /// <param name="boundedUser">User which to bound root folder.</param>
        /// <param name="boundedFolder">Folder which to bound to public folder.</param>
        public void UpdateSharedFolderRoot(string rootID,bool enabled,string folder,string description,SharedFolderRootType_enum rootType,string boundedUser,string boundedFolder)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that root ID exists.
                *) If root folder name is changed, ensure that new root folder won't conflict 
                   any other root folder name. Throw Exception if does.     
                *) Update root folder.
            */

            if(rootID == null || rootID == ""){
                throw new Exception("Invalid rootID value, rootID can't be '' or null !");
            }

            //--- Validate values -------------------------------------//
            ArgsValidator.ValidateNotNull(rootID);
            ArgsValidator.ValidateSharedFolderRoot(folder);
            ArgsValidator.ValidateNotNull(description);
            if(rootType == SharedFolderRootType_enum.BoundedRootFolder){
                ArgsValidator.ValidateUserName(boundedUser);
                ArgsValidator.ValidateFolder(boundedFolder);
            }
            //---------------------------------------------------------//

            /* We handle these is SQL, sql returns these errors in ErrorText
             
            *) Ensure that root ID exists.
                *) If root folder name is changed, ensure that new root folder won't conflict 
                   any other root folder name. Throw Exception if does. 
            */

            // Update root folder
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateSharedFolderRoot")){
				sqlCmd.AddParameter("_rootID"        ,NpgsqlDbType.Varchar,rootID);
				sqlCmd.AddParameter("_enabled"       ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_folder"        ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_description"   ,NpgsqlDbType.Varchar,description);
                sqlCmd.AddParameter("_rootType"      ,NpgsqlDbType.Integer,rootType);
                sqlCmd.AddParameter("_boundedUser"   ,NpgsqlDbType.Varchar,boundedUser);
                sqlCmd.AddParameter("_boundedFolder" ,NpgsqlDbType.Varchar,boundedFolder);
							
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion


		#region method GetFolderACL

		/// <summary>
		/// Gets specified folder ACL.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what ACL to get. For example: Inbox,Public Folders/Documents .</param>
		/// <returns></returns>
		public DataView GetFolderACL(string accessingUser,string folderOwnerUser,string folder)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'a' permission.
                   There is builtin user system, skip ACL for it.
                *) Get folder ACL.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder doesn't exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'a' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.a) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetFolderACL")){
                sqlCmd.AddParameter("_folderOwnerUser",NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folderName"     ,NpgsqlDbType.Varchar,folder);
				
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "ACL";

				return ds.Tables["ACL"].DefaultView;
			}
		}

		#endregion

		#region method DeleteFolderACL

		/// <summary>
		/// Deletes specified folder ACL for specified user.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what ACL to delete. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="userOrGroup">User or user group which ACL on specified folder to delete.</param>
		public void DeleteFolderACL(string accessingUser,string folderOwnerUser,string folder,string userOrGroup)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that folder owner user exists.
                *) Ensure that user or user group exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'a' permission.
                   There is builtin user system, skip ACL for it.
                *) Delete folder.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateUserName(userOrGroup);
            //---------------------------------------//

            // Ensure that folder owner user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Ensure that user or user group exists. Skip check for anyone.
            if(userOrGroup.ToLower() != "anyone" && !GroupExists(userOrGroup)){
                if(!UserExists(userOrGroup)){
                    throw new Exception("Invalid userOrGroup value, there is no such user or group '" + userOrGroup + "' !");
                }
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder doesn't exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'a' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.a) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Delete folder ACL
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteFolderACL")){
                sqlCmd.AddParameter("_folderOwnerUser",NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folderName"     ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_userOrGroup"    ,NpgsqlDbType.Varchar,userOrGroup);
				
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method SetFolderACL

		/// <summary>
		/// Sets specified folder ACL for specified user.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what ACL to set. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="userOrGroup">>User or user which group ACL set to specified folder.</param>
		/// <param name="setType">Specifies how ACL flags must be stored (ADD,REMOVE,REPLACE).</param>
		/// <param name="aclFlags">ACL flags.</param>
		public void SetFolderACL(string accessingUser,string folderOwnerUser,string folder,string userOrGroup,IMAP_Flags_SetType setType,IMAP_ACL_Flags aclFlags)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that folder owner user exists.
                *) Ensure that user or user group exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'a' permission.
                   There is builtin user system, skip ACL for it.
                *) Set folder ACL folder.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateUserName(userOrGroup);
            //---------------------------------------//

            // Ensure that folder owner user exists.
            if(!UserExists(folderOwnerUser)){
               throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Ensure that user or user group exists. Skip check for anyone.
            if(userOrGroup.ToLower() != "anyone" && !GroupExists(userOrGroup)){
                if(!UserExists(userOrGroup)){
                   throw new Exception("Invalid userOrGroup value, there is no such user or group '" + userOrGroup + "' !");
               }
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
               folderOwnerUser = mappedFolder.FolderOnwer;
               folder = mappedFolder.Folder;

               if(folderOwnerUser == "" || folder == ""){
                   throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
               }
            }

            // Ensure that folder doesn't exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
               throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'a' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
               IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
               if((acl & IMAP_ACL_Flags.a) == 0){
                   throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
               }
            }

            //--- Set folder ACL
			IMAP_ACL_Flags currentACL_Flags = GetUserACL(folderOwnerUser,folder,userOrGroup);
			if(setType == IMAP_Flags_SetType.Replace){
				currentACL_Flags = aclFlags;
			}
			else if(setType == IMAP_Flags_SetType.Add){
				currentACL_Flags |= aclFlags;
			}
			else if(setType == IMAP_Flags_SetType.Remove){
				currentACL_Flags &= ~aclFlags;
			}
	
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_SetFolderACL")){
                sqlCmd.AddParameter("_folderOwnerUser",NpgsqlDbType.Varchar,folderOwnerUser);
				sqlCmd.AddParameter("_folderName"     ,NpgsqlDbType.Varchar,folder);
				sqlCmd.AddParameter("_userOrGroup"    ,NpgsqlDbType.Varchar,userOrGroup);
				sqlCmd.AddParameter("_permissions"    ,NpgsqlDbType.Varchar,IMAP_Utils.ACL_to_String(currentACL_Flags));
				
				DataSet ds = sqlCmd.Execute();
			}
		}

		#endregion

		#region method GetUserACL

		/// <summary>
		/// Gets what permissions specified user has to specified folder.
		/// </summary>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
		/// <param name="folder">Folder which ACL to get. For example Inbox,Public Folders.</param>
		/// <param name="user">User name which ACL to get.</param>
		/// <returns></returns>
		public IMAP_ACL_Flags GetUserACL(string folderOwnerUser,string folder,string user)
		{
            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateUserName(user);
            //---------------------------------------//

            /*
                *) Ensure that folder owner user exists.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
            // ??    *) Ensure that folder exists. Throw Exception if don't.
                 
                   If folder owner is user, and no permissions explicity set, then user have ALL permissions.
                   If user isn't folder owner:
                     *) Try to get User ACl. Also look user groups.
                        If doesn't exist, try to anyone ACL.                           
                        If anyone ACL doesn't exist, try to inherit ACL from parent folders.
                        
                      NOTE: Get maximum ACL user has. For example if user has explicity ACL set and
                            and has Group ACL set, then ACL = max combination of ACL falgs.
            */

            // Ensure that folder owner user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Ensure that folder user exists.
            if(!UserExists(user)){
                throw new Exception("User '" + user + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            IMAP_ACL_Flags userACL = IMAP_ACL_Flags.None;
                
            // See if ACL is set to this folder, if not show inhereted ACL
            DataView dv = null;
            try{
                dv = GetFolderACL("system",folderOwnerUser,folder);
            }
            // Folder doesnt exist, just skip it
            catch{
            }                
            if(dv != null && dv.Count > 0){
                bool aclSetToUser = false;
                foreach(DataRowView drV in dv){
                    // This is group, user is member of that group
                    if(GroupExists(drV["User"].ToString()) && IsUserGroupMember(drV["User"].ToString(),user)){
                        userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
                    } 
                    // ACL is explicity set to user
                    else if(drV["User"].ToString().ToLower() == user.ToLower()){
			            userACL = IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
                        aclSetToUser = true;
				    }
                    // There is ANYONE access
					else if(drV["User"].ToString().ToLower() == "anyone"){
					    userACL = IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
				    }
                }

                // ACL isn't explicity set to folder owner user,give all permissions to folder owner user.
                if(!aclSetToUser && user.ToLower() == folderOwnerUser.ToLower()){
                    userACL = IMAP_ACL_Flags.All;
                }
            }
            else{
                // ACL isn't set and user owner, give full rights
                if(user.ToLower() == folderOwnerUser.ToLower()){
                    userACL = IMAP_ACL_Flags.All;
                }
                else{
                    // Try to inherit ACL from parent folder(s)
                    // Move right to left in path.
                    while(folder.LastIndexOf('/') > -1){
                        // Move 1 level to right in path
                        folder = folder.Substring(0,folder.LastIndexOf('/'));

                        dv = GetFolderACL("system",folderOwnerUser,folder);
                        if(dv.Count > 0){                                
                            foreach(DataRowView drV in dv){
                                string userOrGroup = drV["User"].ToString();
                                // This is group, user is member of that group
                                if(GroupExists(userOrGroup) && IsUserGroupMember(drV["User"].ToString(),user)){
                                    userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
                                }
                                // ACL is explicity set to user
                                else if(drV["User"].ToString().ToLower() == user.ToLower()){
			    		            userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
				                }
                                // There is ANYONE access
					            else if(drV["User"].ToString().ToLower() == "anyone"){
						            userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
						        }
                            }

                            // We inhereted all permission, don't look other parent folders anymore
                            break;
                        }
                    }
                }
            }

			return userACL;
		}

		#endregion


        #region method CreateUserDefaultFolders

        /// <summary>
        /// Creates specified user default folders, if they don't exist already.
        /// </summary>
        /// <param name="userName">User name to who's default folders to create.</param>
        public void CreateUserDefaultFolders(string userName)
        {
            foreach(DataRowView drV in GetUsersDefaultFolders()){
                if(!FolderExists(userName + "/" + drV["FolderName"].ToString())){
                    CreateFolder("system",userName,drV["FolderName"].ToString());
                    SubscribeFolder(userName,drV["FolderName"].ToString());
                }
            }
        }
        
        #endregion

        #region method GetUsersDefaultFolders

        /// <summary>
        /// Gets users default folders.
        /// </summary>
        /// <returns></returns>
        public DataView GetUsersDefaultFolders()
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUsersDefaultFolders")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "UsersDefaultFolders";

                // Force Inbox to be created
                if(ds.Tables["UsersDefaultFolders"].Rows.Count == 0){
                    DataRow dr = ds.Tables["UsersDefaultFolders"].NewRow();
				    dr["FolderName"] = "Inbox";
				    dr["Permanent"]  = true;									
				    ds.Tables["UsersDefaultFolders"].Rows.Add(dr);
                }

				return ds.Tables["UsersDefaultFolders"].DefaultView;
			}
        }

        #endregion

        #region method AddUsersDefaultFolder

        /// <summary>
        /// Adds users default folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <param name="permanent">Spcifies if folder is permanent, user can't delete it.</param>
        public void AddUsersDefaultFolder(string folderName,bool permanent)
        {
            ArgsValidator.ValidateFolder(folderName);

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddUsersDefaultFolder")){
				sqlCmd.AddParameter("_folderName" ,NpgsqlDbType.Varchar,folderName);
				sqlCmd.AddParameter("_permanent  ",NpgsqlDbType.Boolean ,permanent);
							
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "UsersDefaultFolders";
			}
        }

        #endregion

        #region method DeleteUsersDefaultFolder

        /// <summary>
        /// Deletes specified users default folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        public void DeleteUsersDefaultFolder(string folderName)
        {
            ArgsValidator.ValidateFolder(folderName);

            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteUsersDefaultFolder")){
				sqlCmd.AddParameter("_folderName" ,NpgsqlDbType.Varchar,folderName);
							
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "UsersDefaultFolders";
			}
        }

        #endregion


		#region method GetMailboxSize

		/// <summary>
		/// Gets specified user mailbox size.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns>Returns mailbox size.</returns>
		public long GetMailboxSize(string userName)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMailboxSize")){
				sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,userName);

                object result = sqlCmd.ExecuteScalar();
                if(result == DBNull.Value){
                    return 0;
                }
                else{
                    return (long)result;
                }
			}
		}

		#endregion


        #region method GetRecycleBinSettings

        /// <summary>
        /// Gets recycle bin settings.
        /// </summary>
        /// <returns></returns>
        public DataTable GetRecycleBinSettings()
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetRecycleBinSettings")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "RecycleBinSettings";

                if(ds.Tables["RecycleBinSettings"].Rows.Count == 0){
                    DataRow dr = ds.Tables["RecycleBinSettings"].NewRow();
                    dr["DeleteToRecycleBin"]  = false;
                    dr["DeleteMessagesAfter"] = 1;
                    ds.Tables["RecycleBinSettings"].Rows.Add(dr);
                }

				return ds.Tables["RecycleBinSettings"];
			}
        }

        #endregion

        #region method UpdateRecycleBinSettings

        /// <summary>
        /// Updates recycle bin settings.
        /// </summary>
        /// <param name="deleteToRecycleBin">Specifies if deleted messages are store to recycle bin.</param>
        /// <param name="deleteMessagesAfter">Specifies how old messages will be deleted.</param>
        public void UpdateRecycleBinSettings(bool deleteToRecycleBin,int deleteMessagesAfter)
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateRecycleBinSettings")){
                sqlCmd.AddParameter("_deleteToRecycleBin" ,NpgsqlDbType.Boolean,deleteToRecycleBin);
                sqlCmd.AddParameter("_deleteMessagesAfter",NpgsqlDbType.Integer,deleteMessagesAfter);
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method GetRecycleBinMessagesInfo

		/// <summary>
        /// Gets recycle bin messages info. 
        /// </summary>
        /// <param name="user">User who's recyclebin messages to get or null if all users messages.</param>
        /// <param name="startDate">Messages from specified date. Pass DateTime.MinValue if not used.</param>
        /// <param name="endDate">Messages to specified date. Pass DateTime.MinValue if not used.</param>
        /// <returns></returns>
        public DataView GetRecycleBinMessagesInfo(string user,DateTime startDate,DateTime endDate)
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetRecycleBinMessagesInfo")){
                if(user != null && user != ""){
                    sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,user);
                }
                else{
                    sqlCmd.AddParameter("_userName",NpgsqlDbType.Varchar,null);
                }
                sqlCmd.AddParameter("_startDate",NpgsqlDbType.Timestamp,startDate);                
                if(endDate != DateTime.MinValue){
                    sqlCmd.AddParameter("_endDate",NpgsqlDbType.Timestamp,endDate);
                }
                else{
                    sqlCmd.AddParameter("_endDate",NpgsqlDbType.Timestamp,DateTime.MaxValue);
                }
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "MessagesInfo";

				return ds.Tables["MessagesInfo"].DefaultView;
			}
        }
				
		#endregion

        #region method GetRecycleBinMessages

        /// <summary>
        /// Gets recycle bin message stream. NOTE: This method caller must take care of closing stream. 
        /// </summary>
        /// <param name="messageID">Message ID if of message what to get.</param>
        /// <returns></returns>
        public Stream GetRecycleBinMessage(string messageID)
        {
            // Get message and message info from recycle bin
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetRecycleBinMessage")){
                sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,messageID);
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "RecycleBin";
                
				if(ds.Tables["RecycleBin"].Rows.Count > 0){                    
                    return new MemoryStream(((byte[])ds.Tables["RecycleBin"].Rows[0]["Data"]));
                }
                else{
                    throw new Exception("Specified message doesn't exist !");
                }
			}
        }
				
		#endregion

        #region method DeleteRecycleBinMessage

        /// <summary>
        /// Deletes specified recycle bin message.
        /// </summary>
        /// <param name="messageID">Message ID.</param>
        public void DeleteRecycleBinMessage(string messageID)
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteRecycleBinMessage")){
                sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,messageID);
				DataSet ds = sqlCmd.Execute();
			}
        }

        #endregion

        #region method RestoreRecycleBinMessage

        /// <summary>
        /// Restores specified recycle bin message.
        /// </summary>
        /// <param name="messageID">Message ID.</param>
        public void RestoreRecycleBinMessage(string messageID)
        {
            // Get message and message info from recycle bin
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetRecycleBinMessage")){
                sqlCmd.AddParameter("_messageID" ,NpgsqlDbType.Varchar,messageID);
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "RecycleBin";
                
				if(ds.Tables["RecycleBin"].Rows.Count > 0){
                    string userName = ds.Tables["RecycleBin"].Rows[0]["User"].ToString();
                    string folder   = ds.Tables["RecycleBin"].Rows[0]["Folder"].ToString();

                    // If folder doesn't exist, create it
                    if(!FolderExists(userName + "/" + folder)){
                        CreateFolder("system",userName,folder);
                    }

                    // Try to store message back
                    StoreMessage(
                        "system",
                        userName,
                        folder,
                        new MemoryStream(((byte[])ds.Tables["RecycleBin"].Rows[0]["Data"])),
                        DateTime.Now,
                        IMAP_MessageFlags.Recent
                    );
                }
			}
        }

        #endregion

		#endregion

	
		#region Security related

		#region method GetSecurityList

		/// <summary>
		/// Gets security entries list.
		/// </summary>
		public DataView GetSecurityList()
		{			
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetSecurityList")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "IPSecurity";

				return ds.Tables["IPSecurity"].DefaultView;
			}
		}
		
		#endregion


		#region method AddSecurityEntry

		/// <summary>
		/// Adds new IP security entry.
		/// </summary>
		/// <param name="id">IP security entry ID.</param>
		/// <param name="enabled">Specifies if IP security entry is enabled.</param>
		/// <param name="description">IP security entry description text.</param>
		/// <param name="service">Specifies service for what security entry applies.</param>
		/// <param name="action">Specifies what action done if IP matches to security entry range.</param>
		/// <param name="startIP">Range start IP.</param>
		/// <param name="endIP">Range end IP.</param>
		public void AddSecurityEntry(string id,bool enabled,string description,Service_enum service,IPSecurityAction_enum action,IPAddress startIP,IPAddress endIP)
		{
			if(id.Length == 0){
				throw new Exception("You must specify securityID");
			}
            
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddSecurityEntry")){
				sqlCmd.AddParameter("_id"          ,NpgsqlDbType.Varchar,id);
                sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_service"     ,NpgsqlDbType.Integer,(int)service);
				sqlCmd.AddParameter("_action"      ,NpgsqlDbType.Integer,(int)action);
				sqlCmd.AddParameter("_startIP"     ,NpgsqlDbType.Varchar,startIP.ToString());
				sqlCmd.AddParameter("_endIP"       ,NpgsqlDbType.Varchar,endIP.ToString());
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method DeleteSecurityEntry

		/// <summary>
		/// Deletes security entry.
		/// </summary>
		/// <param name="id">IP security entry ID.</param>
		public void DeleteSecurityEntry(string id)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteSecurityEntry")){
				sqlCmd.AddParameter("_securityID" ,NpgsqlDbType.Varchar,id);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method UpdateSecurityEntry

		/// <summary>
		/// Updates IP security entry.
		/// </summary>
		/// <param name="id">IP security entry ID.</param>
		/// <param name="enabled">Specifies if IP security entry is enabled.</param>
		/// <param name="description">IP security entry description text.</param>
		/// <param name="service">Specifies service for what security entry applies.</param>
		/// <param name="action">Specifies what action done if IP matches to security entry range.</param>
		/// <param name="startIP">Range start IP.</param>
		/// <param name="endIP">Range end IP.</param>
		public void UpdateSecurityEntry(string id,bool enabled,string description,Service_enum service,IPSecurityAction_enum action,IPAddress startIP,IPAddress endIP)
		{            
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateSecurityEntry")){
				sqlCmd.AddParameter("_id"          ,NpgsqlDbType.Varchar,id);
                sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_service"     ,NpgsqlDbType.Integer,(int)service);
				sqlCmd.AddParameter("_action"      ,NpgsqlDbType.Integer,(int)action);
				sqlCmd.AddParameter("_startIP"     ,NpgsqlDbType.Varchar,startIP.ToString());
				sqlCmd.AddParameter("_endIP"       ,NpgsqlDbType.Varchar,endIP.ToString());
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#endregion


		#region Filters related

		#region method GetFilters

		/// <summary>
		/// Gets filter list.
		/// </summary>
		/// <returns></returns>
		public DataView GetFilters()
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetFilters")){				
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "SmtpFilters";

				return ds.Tables["SmtpFilters"].DefaultView;
			}
		}
		
		#endregion


		#region method AddFilter

		/// <summary>
		/// Adds new filter.
		/// </summary>
		/// <param name="filterID">Filter ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="description">Filter description</param>
		/// <param name="type">Filter type. Eg. ISmtpMessageFilter.</param>
		/// <param name="assembly">Assembly with full location. Eg. C:\MailServer\Filters\filter.dll .</param>
		/// <param name="className">Filter full class name, wih namespace. Eg. LumiSoft.MailServer.Fileters.Filter1 .</param>
		/// <param name="cost">Filters are sorted by cost and proccessed with cost value. Smallest cost is proccessed first.</param>
		/// <param name="enabled">Specifies if filter is enabled.</param>
		/// <remarks>Throws exception if specified filter entry already exists.</remarks>
		public void AddFilter(string filterID,string description,string type,string assembly,string className,long cost,bool enabled)
		{
			if(filterID.Length == 0){
				throw new Exception("You must specify filterID");
			}

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddFilter")){
				sqlCmd.AddParameter("_filterID"    ,NpgsqlDbType.Varchar,filterID);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_type"        ,NpgsqlDbType.Varchar,type);
				sqlCmd.AddParameter("_assembly"    ,NpgsqlDbType.Varchar,assembly);
				sqlCmd.AddParameter("_className"   ,NpgsqlDbType.Varchar,className);
				sqlCmd.AddParameter("_cost"        ,NpgsqlDbType.Bigint ,cost);
				sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method DeleteFilter

		/// <summary>
		/// Deletes specified filter.
		/// </summary>
		/// <param name="filterID">FilterID of the filter which to delete.</param>
		public void DeleteFilter(string filterID)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteFilter")){
				sqlCmd.AddParameter("_filterID" ,NpgsqlDbType.Varchar,filterID);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#region method UpdateFilter

		/// <summary>
		/// Updates specified filter.
		/// </summary>		
		/// <param name="filterID">FilterID which to update.</param>
		/// <param name="description">Filter description</param>
		/// <param name="type">Filter type. Eg. ISmtpMessageFilter.</param>
		/// <param name="assembly">Assembly with full location. Eg. C:\MailServer\Filters\filter.dll .</param>
		/// <param name="className">Filter full class name, wih namespace. Eg. LumiSoft.MailServer.Fileters.Filter1 .</param>
		/// <param name="cost">Filters are sorted by cost and proccessed with cost value. Smallest cost is proccessed first.</param>
		/// <param name="enabled">Specifies if filter is enabled.</param>
		/// <returns></returns>
		public void UpdateFilter(string filterID,string description,string type,string assembly,string className,long cost,bool enabled)
		{
			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateFilter")){
				sqlCmd.AddParameter("_filterID"    ,NpgsqlDbType.Varchar,filterID);
				sqlCmd.AddParameter("_description" ,NpgsqlDbType.Varchar,description);
				sqlCmd.AddParameter("_type"        ,NpgsqlDbType.Varchar,type);
				sqlCmd.AddParameter("_assembly"    ,NpgsqlDbType.Varchar,assembly);
				sqlCmd.AddParameter("_className"   ,NpgsqlDbType.Varchar,className);
				sqlCmd.AddParameter("_cost"        ,NpgsqlDbType.Bigint ,cost);
				sqlCmd.AddParameter("_enabled"     ,NpgsqlDbType.Boolean,enabled);
							
				DataSet ds = sqlCmd.Execute();
			}
		}
		
		#endregion

		#endregion


		#region Settings related

		#region method GetSettings

		/// <summary>
		/// Gets server settings.
		/// </summary>
		/// <returns></returns>
		public DataRow GetSettings()
		{
			DataSet ds = new DataSet();
			API_Utlis.CreateSettingsSchema(ds);

			using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetSettings")){
				DataSet dsS = sqlCmd.Execute();
				dsS.Tables[0].TableName = "Settings";

				if(dsS.Tables["Settings"].Rows.Count > 0){
					byte[] data = (byte[])dsS.Tables["Settings"].Rows[0]["Settings"];

					ds.ReadXml(new MemoryStream(data));
				}		
			}

            if(ds.Tables["Settings"].Rows.Count == 0){
				ds.Tables["Settings"].Rows.Add(ds.Tables["Settings"].NewRow());
			}
			
			foreach(DataRow dr in ds.Tables["Settings"].Rows){
				foreach(DataColumn dc in ds.Tables["Settings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

            if(ds.Tables["SMTP_Bindings"].Rows.Count == 0){
                ds.Tables["SMTP_Bindings"].Rows.Add(ds.Tables["SMTP_Bindings"].NewRow());
            }
            foreach(DataRow dr in ds.Tables["SMTP_Bindings"].Rows){
				foreach(DataColumn dc in ds.Tables["SMTP_Bindings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

            if(ds.Tables["POP3_Bindings"].Rows.Count == 0){
                ds.Tables["POP3_Bindings"].Rows.Add(ds.Tables["POP3_Bindings"].NewRow());
            }
            foreach(DataRow dr in ds.Tables["POP3_Bindings"].Rows){
				foreach(DataColumn dc in ds.Tables["POP3_Bindings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

            if(ds.Tables["IMAP_Bindings"].Rows.Count == 0){
                ds.Tables["IMAP_Bindings"].Rows.Add(ds.Tables["IMAP_Bindings"].NewRow());
            }
            foreach(DataRow dr in ds.Tables["IMAP_Bindings"].Rows){
				foreach(DataColumn dc in ds.Tables["IMAP_Bindings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}
            			
			return ds.Tables["Settings"].Rows[0];
		}

		#endregion

		#region method UpdateSettings

		/// <summary>
		/// Updates server settings.
		/// </summary>
		public void UpdateSettings(DataRow settings)
		{/*
			DataRow row = GetSettings();
			foreach(DataColumn dc in row.Table.Columns){
				row[dc.ColumnName] = settings[dc.ColumnName];
			}*/

			using(MemoryStream strm = new MemoryStream()){
				settings.Table.DataSet.WriteXml(strm);

				using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateSettings")){
					sqlCmd.AddParameter("_settings" ,NpgsqlDbType.Bytea,strm.ToArray());
								
					DataSet ds = sqlCmd.Execute();
				}
			}
		}

		#endregion

		#endregion



        #region method GetACL

        /// <summary>
        /// Gets all folders IMAP acl entries.
        /// </summary>
        /// <returns></returns>
        private DataSet GetACL()
        {
            using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"select * from lsIMAP_ACL")){
				DataSet ds = sqlCmd.Execute();
				ds.Tables[0].TableName = "ACL";

				return ds;
			}
        }

        #endregion

        #region method IsUserGroupMember

        /// <summary>
        /// Gets if specified user is specified user group member. Returns true if user is user group member.
        /// </summary>
        /// <param name="group">User group name.</param>
        /// <param name="user">User name.</param>
        /// <returns>Returns true if user is user group member.</returns>
        private bool IsUserGroupMember(string group,string user)
        {
            List<string> proccessedGroups = new List<string>();
            Queue<string> membersQueue = new Queue<string>();
            string[] members = GetGroupMembers(group);
            foreach(string member in members){
                membersQueue.Enqueue(member);
            }

            while(membersQueue.Count > 0){
                string member = membersQueue.Dequeue();
                // Nested group
                DataRow drGroup = GetGroup(member);
                if(drGroup != null){
                    // Don't proccess poroccessed groups any more, causes infinite loop
                    if(!proccessedGroups.Contains(member.ToLower())){
                        // Skip disabled groups
                        if(Convert.ToBoolean(drGroup["Enabled"])){
                            members = GetGroupMembers(member);
                            foreach(string m in members){
                                membersQueue.Enqueue(m);
                            }  
                        }

                        proccessedGroups.Add(member.ToLower());
                    }
                }
                // User
                else{
                    if(member.ToLower() == user.ToLower()){
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region method MapSharedFolder

        /// <summary>
        /// This call holds shaared folder mapping info.
        /// </summary>
        private class SharedFolderMapInfo
        {
            private string m_OriginalFolder = "";
            private string m_FolderOwner    = "";
            private string m_Folder         = "";
            private string m_SharedRootName = "";

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="originalFolder"></param>
            /// <param name="folderOwner"></param>
            /// <param name="folder"></param>
            /// <param name="sharedRootName"></param>
            public SharedFolderMapInfo(string originalFolder,string folderOwner,string folder,string sharedRootName)
            {
                m_OriginalFolder = originalFolder;
                m_FolderOwner    = folderOwner;
                m_Folder         = folder;
                m_SharedRootName = sharedRootName;
            }


            #region Properties Implementation

            /// <summary>
            /// Gets original folder.
            /// </summary>
            public string OriginalFolder
            {
                get{ return m_OriginalFolder; }
            }

            /// <summary>
            /// Shared folder owner. This is available only if OriginalFolder is shared folder.
            /// </summary>
            public string FolderOnwer
            {
                get{ return m_FolderOwner; }
            }

            /// <summary>
            /// Gets shared folder owner folder. This is available only if OriginalFolder is shared folder.
            /// </summary>
            public string Folder
            {
                get{ return m_Folder; }
            }

            /// <summary>
            /// Gets shared root folder name. This is available only if OriginalFolder is shared folder.
            /// </summary>
            public string SharedRootName
            {
                get{ return m_SharedRootName; }
            }

            /// <summary>
            /// Gets if OriginalFolder is shared folder.
            /// </summary>
            public bool IsSharedFolder
            {
                get{ return m_SharedRootName != ""; }
            }

            #endregion
        }

        /// <summary>
        /// If folder is Shared Folder, then maps folder to actual account.
        /// </summary>
        /// <param name="folder">Folder to map.</param>
        /// <returns></returns>
        private SharedFolderMapInfo MapSharedFolder(string folder)
        {
            string rootFolder = folder.Split(new char[]{'/'},2)[0];

            SharedFolderRoot[] roots = GetSharedFolderRoots();
            foreach(SharedFolderRoot root in roots){
                if(root.RootType == SharedFolderRootType_enum.BoundedRootFolder){
                    if(rootFolder.ToLower() == root.FolderName.ToLower()){
                    //  folder = root.BoundedUser + "/" + root.BoundedFolder + folder.Substring(root.FolderName.Length);
                        return new SharedFolderMapInfo(folder,root.BoundedUser,root.BoundedFolder,root.FolderName);
                    }
                }
                else if(root.RootType == SharedFolderRootType_enum.UsersSharedFolder){
                    if(rootFolder.ToLower() == root.FolderName.ToLower()){
                        // Cut off root folder name
                        string[] p = folder.Split(new char[]{'/'},3);
                        // root/user/folder
                        if(p.Length == 3){
                            return new SharedFolderMapInfo(folder,p[1],p[2],root.FolderName);
                        }
                        // root/user
                        else if(p.Length == 3){
                            return new SharedFolderMapInfo(folder,p[1],"",root.FolderName);
                        }
                        // root (User and folder missing)
                        else{
                            return new SharedFolderMapInfo(folder,"","",root.FolderName);
                        }
                    }
                }
            }

            return new SharedFolderMapInfo(folder,"","","");
        }

        #endregion

        #region method GetGroup

        /// <summary>
        /// Gets specified user group. If group doesn't exist, returns null;
        /// </summary>
        /// <param name="group">User group name.</param>
        /// <returns></returns>
        private DataRow GetGroup(string group)
        {
		    foreach(DataRowView drV in GetGroups()){
                if(group.ToLower() == drV["GroupName"].ToString().ToLower()){
                    return drV.Row;
                }
            }

            return null;
        }

        #endregion

		#region function GetTopLines

		private byte[] GetTopLines(Stream strm,int nrLines)
		{
			TextReader reader = (TextReader)new StreamReader(strm);
			
			strm.Position = 0;

			int  lCounter = 0;
			int  msgLine  = -1;
			bool msgLines = false;
			StringBuilder strBuilder = new StringBuilder();
			while(true){
				string line = reader.ReadLine();

				// Reached end of message
				if(line == null){
					break;
				}
				else{
					// End of header reached
					if(!msgLines && line.Length == 0){
						// Set flag, that message lines reading start.
						msgLines = true;
					}

					// Check that wanted message lines count isn't exceeded
					if(msgLines){
						if(msgLine > nrLines){
							break;
						}
						msgLine++;
					}

					strBuilder.Append(line + "\r\n");
				}

				lCounter++;
			}
	
			return System.Text.Encoding.ASCII.GetBytes(strBuilder.ToString());			
		}

		#endregion
	}
}

using Common.Logging;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Web.Hosting;

namespace Owin.ApiGateway.Logger
{
    public class SqlServerRequestResponseLogWriter : IRequestResponseLogStoreWriter
    {
        private string connectionString;

        private ILog logger;

        public SqlServerRequestResponseLogWriter(ILog logger)
        {
            this.logger = logger;
        }

        private string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(this.connectionString))
                {
                    connectionString = ConfigurationManager.ConnectionStrings["RequestResponseLogger"].ConnectionString;
                }

                return this.connectionString;
            }
        }

        public void SaveLogData(LogEntry logEntry)
        {
            string responseString;

            if (logEntry.IsResponseGziped)
            {
                responseString = GzipDecompress(logEntry.ResponseArray, logEntry.IsChunkedTransferEncoding);
            }
            else
            {
                responseString = System.Text.Encoding.UTF8.GetString(logEntry.ResponseArray);
            }

            using (var connection = new SqlConnection(this.ConnectionString))
            {
                try
                {
                    connection.Open();

                    string commandString = "INSERT INTO WebServiceLog(DateTime, RequestedUrl, SoapAction, RequestString, RequestHeaders, ResponseString, ResponseHeaders, IsFromCache, ResponseTimeInMS)" +
                        "VALUES (@DateTime, @RequestedUrl, @SoapAction, @RequestString, @RequestHeaders, @ResponseString, @ResponseHeaders, @IsFromCache, @ResponseTimeInMS)";

                    var cmd = new SqlCommand(commandString, connection);
                    cmd.Parameters.Add("DateTime", SqlDbType.DateTime).Value = logEntry.DateTime;
                    cmd.Parameters.Add("RequestedUrl", SqlDbType.VarChar).Value = logEntry.RequestedUrl;
                    cmd.Parameters.Add("SoapAction", SqlDbType.VarChar).Value = logEntry.SoapAction == null ? DBNull.Value : (object)logEntry.SoapAction;
                    cmd.Parameters.Add("RequestString", SqlDbType.VarChar).Value = logEntry.RequestString;
                    cmd.Parameters.Add("RequestHeaders", SqlDbType.VarChar).Value = logEntry.RequestHeaders;
                    cmd.Parameters.Add("ResponseString", SqlDbType.VarChar).Value = responseString;
                    cmd.Parameters.Add("ResponseHeaders", SqlDbType.VarChar).Value = logEntry.ResponseHeaders;
                    cmd.Parameters.Add("IsFromCache", SqlDbType.Bit).Value = logEntry.IsFromCache;
                    cmd.Parameters.Add("ResponseTimeInMS", SqlDbType.Int).Value = logEntry.ResponseTimeInMS;

                    cmd.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    this.logger.Error("An exception was thrown in SaveLogData.", e);
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private string GzipDecompress(byte[] responseBytes, bool isChunkedTransferEncoding)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(responseBytes, 0, responseBytes.Length);
                ms.Flush();

                ms.Position = 0;

                // This is required to fix bug that occures when ApiGateway is hosted in IIS and response is sent in chunks 
                // --> chunk header was defined in ProxyMiddleware
                bool isChunkedTransferEncodingInHostedMode = HostingEnvironment.IsHosted && isChunkedTransferEncoding;

                StreamReader chunkHeaderReader = null;
                try {
                    
                    if (isChunkedTransferEncodingInHostedMode)
                    {
                        // move stream forward. We must skip chunk header - first line terminated with \r\n

                        chunkHeaderReader = new StreamReader(ms);
                        char[] buffer = new char[1];
                        bool stop = false;
                        int chunkHeaderLenght = 0;
                        while (!stop) {
                            chunkHeaderReader.Read(buffer, 0, 1);
                            chunkHeaderLenght++;

                            if (buffer[0] == '\r')
                            {
                                // read next character. It should be \n
                                chunkHeaderReader.Read(buffer, 0, 1);
                                chunkHeaderLenght++;
                                stop = true;
                            }
                        }

                        ms.Position = chunkHeaderLenght;
                    }

                    using (var gzipStream = new GZipStream(ms, CompressionMode.Decompress))
                    using (var sr = new StreamReader(gzipStream))
                    {
                        var decompressedContent = sr.ReadToEnd();

                        return decompressedContent;
                    }
                }
                finally
                {
                    if (chunkHeaderReader != null)
                    {
                        chunkHeaderReader.Dispose();
                    }
                }
            }
        }
    }
}

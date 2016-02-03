using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Server
{
    /// <summary>
    /// Provides data to event GetMessageItems.
    /// </summary>
    public class IMAP_eArgs_MessageItems
    {
        private IMAP_Session           m_pSession           = null;
        private IMAP_Message           m_pMessageInfo       = null;
        private IMAP_MessageItems_enum m_MessageItems       = IMAP_MessageItems_enum.Message;
        private bool                   m_CloseMessageStream = true;
        private Stream                 m_MessageStream      = null;
        private long                   m_MessageStartOffset = 0;
        private byte[]                 m_Header             = null;
        private string                 m_Envelope           = null;
        private string                 m_BodyStructure      = null;
        private bool                   m_MessageExists      = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to current IMAP session.</param>
        /// <param name="messageInfo">Message info what message items to get.</param>
        /// <param name="messageItems">Specifies message items what must be filled.</param>
        public IMAP_eArgs_MessageItems(IMAP_Session session,IMAP_Message messageInfo,IMAP_MessageItems_enum messageItems)
        {
            m_pSession     = session;
            m_pMessageInfo = messageInfo;
            m_MessageItems = messageItems;
        }

        /// <summary>
        /// Default deconstructor.
        /// </summary>
        ~IMAP_eArgs_MessageItems()
        {
            Dispose();
        }

        #region method Dispose

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if(m_CloseMessageStream && m_MessageStream != null){
                m_MessageStream.Dispose();
                m_MessageStream = null;
            }
        }

        #endregion


        #region internal method Validate

        /// <summary>
        /// Checks that all required data items are provided, if not throws exception.
        /// </summary>
        internal void Validate()
        {
            if((m_MessageItems & IMAP_MessageItems_enum.BodyStructure) != 0 && m_BodyStructure == null){
                throw new Exception("IMAP BODYSTRUCTURE is required, but not provided to IMAP server component !");
            }
            if((m_MessageItems & IMAP_MessageItems_enum.Envelope) != 0 && m_Envelope == null){
                throw new Exception("IMAP ENVELOPE is required, but not provided to IMAP server component  !");
            }
            if((m_MessageItems & IMAP_MessageItems_enum.Header) != 0 && m_Header == null){
                throw new Exception("Message header is required, but not provided to IMAP server component  !");
            }
            if((m_MessageItems & IMAP_MessageItems_enum.Message) != 0 && m_MessageStream == null){
                throw new Exception("Full message is required, but not provided to IMAP server component  !");
            }
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets reference to current IMAP session.
        /// </summary>
        public IMAP_Session Session
        {
            get{ return m_pSession; }
        }

        /// <summary>
        /// Gets message info what message items to get.
        /// </summary>
        public IMAP_Message MessageInfo
        {
            get{ return m_pMessageInfo; }
        }

        /// <summary>
        /// Gets what message items must be filled.
        /// </summary>
        public IMAP_MessageItems_enum MessageItems
        {
            get{ return m_MessageItems; }
        }

        /// <summary>
        /// Gets or sets if message stream is closed automatically if all actions on it are completed.
        /// Default value is true.
        /// </summary>
        public bool CloseMessageStream
        {
            get{ return m_CloseMessageStream; }

            set{ m_CloseMessageStream = value; }
        }

        /// <summary>
        /// Gets or sets message stream. When setting this property Stream position must be where message begins.
        /// Fill this property only if IMAP_MessageItems_enum.Message flag is specified.
        /// </summary>
        public Stream MessageStream
        {
            get{
                if(m_MessageStream != null){
                    m_MessageStream.Position = m_MessageStartOffset;
                }
                return m_MessageStream; 
            }

            set{
                if(value == null){
                    throw new ArgumentNullException("Property MessageStream value can't be null !");
                }
                if(!value.CanSeek){
                    throw new Exception("Stream must support seeking !");
                }

                m_MessageStream = value;
                m_MessageStartOffset = m_MessageStream.Position;
            }
        }

        /// <summary>
        /// Gets message size in bytes.
        /// </summary>
        public long MessageSize
        {
            get{
                if(m_MessageStream == null){
                    throw new Exception("You must set MessageStream property first to use this property !");
                }
                else{
                    return m_MessageStream.Length - m_MessageStream.Position;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets message main header.
        /// Fill this property only if IMAP_MessageItems_enum.Header flag is specified.
        /// </summary>
        public byte[] Header
        {
            get{ return m_Header; }

            set{
                if(value == null){
                    throw new ArgumentNullException("Property Header value can't be null !");
                }

                m_Header = value;
            }
        }

        /// <summary>
        /// Gets or sets IMAP ENVELOPE string.
        /// Fill this property only if IMAP_MessageItems_enum.Envelope flag is specified.
        /// </summary>
        public string Envelope
        {
            get{ return m_Envelope; }

            set{
                if(value == null){
                    throw new ArgumentNullException("Property Envelope value can't be null !");
                }

                m_Envelope = value;
            }
        }

        /// <summary>
        /// Gets or sets IMAP BODYSTRUCTURE string.
        /// Fill this property only if IMAP_MessageItems_enum.BodyStructure flag is specified.
        /// </summary>
        public string BodyStructure
        {
            get{ return m_BodyStructure; }

            set{
                if(value == null){
                    throw new ArgumentNullException("Property BodyStructure value can't be null !");
                }

                m_BodyStructure = value;
            }
        }

        /// <summary>
        /// Gets or sets if message exists. Set this false, if message actually doesn't exist any more.
        /// </summary>
        public bool MessageExists
        {
            get{ return m_MessageExists; }

            set{ m_MessageExists = value; }
        }

        #endregion

    }
}

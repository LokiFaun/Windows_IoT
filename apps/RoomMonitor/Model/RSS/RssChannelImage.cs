using System.Xml.Serialization;

namespace Dashboard.Model.RSS
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class RssChannelImage
    {
        private string m_Url;

        private string m_Title;

        private string m_Link;

        /// <remarks/>
        [XmlElement("url")]
        public string Url
        {
            get
            {
                return m_Url;
            }
            set
            {
                m_Url = value;
            }
        }

        /// <remarks/>
        [XmlElement("title")]
        public string Title
        {
            get
            {
                return m_Title;
            }
            set
            {
                m_Title = value;
            }
        }

        /// <remarks/>
        [XmlElement("link")]
        public string Link
        {
            get
            {
                return m_Link;
            }
            set
            {
                m_Link = value;
            }
        }
    }
}
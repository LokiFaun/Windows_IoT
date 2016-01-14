using System.Xml.Serialization;

namespace Dashboard.Model.RSS
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [XmlRoot("rss", Namespace = "", IsNullable = false)]
    public partial class Rss
    {
        private RssChannel m_Channel;

        private decimal m_Version;

        /// <remarks/>
        [XmlElement("channel")]
        public RssChannel Channel
        {
            get
            {
                return m_Channel;
            }
            set
            {
                m_Channel = value;
            }
        }

        /// <remarks/>
        [XmlAttribute("version")]
        public decimal Version
        {
            get
            {
                return m_Version;
            }
            set
            {
                m_Version = value;
            }
        }
    }
}
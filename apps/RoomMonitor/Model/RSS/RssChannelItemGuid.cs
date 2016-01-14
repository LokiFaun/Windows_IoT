using System.Xml.Serialization;

namespace Dashboard.Model.RSS
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class RssChannelItemGuid
    {
        private bool m_IsPermaLink;

        private string m_Value;

        /// <remarks/>
        [XmlAttribute("isPermaLink")]
        public bool IsPermaLink
        {
            get
            {
                return m_IsPermaLink;
            }
            set
            {
                m_IsPermaLink = value;
            }
        }

        /// <remarks/>
        [XmlText]
        public string Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                m_Value = value;
            }
        }
    }
}
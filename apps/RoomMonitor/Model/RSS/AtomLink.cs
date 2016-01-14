using System.Xml.Serialization;

namespace Dashboard.Model.RSS
{
    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    [XmlRoot(Namespace = "http://www.w3.org/2005/Atom", IsNullable = false)]
    public partial class AtomLink
    {
        private string m_Type;

        private string m_Href;

        private string m_Rel;

        /// <remarks/>
        [XmlAttribute("type")]
        public string Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
            }
        }

        /// <remarks/>
        [XmlAttribute("href")]
        public string Href
        {
            get
            {
                return m_Href;
            }
            set
            {
                m_Href = value;
            }
        }

        /// <remarks/>
        [XmlAttribute("rel")]
        public string Rel
        {
            get
            {
                return m_Rel;
            }
            set
            {
                m_Rel = value;
            }
        }
    }
}
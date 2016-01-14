using System.Xml.Serialization;

namespace Dashboard.Model.RSS
{
    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://search.yahoo.com/mrss/")]
    [XmlRoot(Namespace = "http://search.yahoo.com/mrss/", IsNullable = false)]
    public partial class Thumbnail
    {
        private string m_Url;

        /// <remarks/>
        [XmlAttribute("url")]
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
    }
}
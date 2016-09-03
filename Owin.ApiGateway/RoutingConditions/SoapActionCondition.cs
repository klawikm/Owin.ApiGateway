namespace Owin.ApiGateway.RoutingConditions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;
    public class SoapActionCondition : RoutingCondition
    {
        private string[] requiredSoapActionRegexStrings;

        public SoapActionCondition()
        {
        }

        public SoapActionCondition(string[] requiredSoapActions)
        {
            this.RequiredSoapActions = requiredSoapActions;
        }

        public SoapActionCondition(string[] requiredSoapActions, string[] requiredSoapActionsRegex)
        {
            this.RequiredSoapActions = requiredSoapActions;
            this.RequiredSoapActionRegexStrings = requiredSoapActionsRegex;
        }

        [XmlElement("RequiredSoapAction")]
        public string[] RequiredSoapActions { get; set; }

        [XmlElement("RequiredSoapActionRegex")]
        public string[] RequiredSoapActionRegexStrings
        {
            get
            {
                return this.requiredSoapActionRegexStrings;
            }
            set
            {
                this.requiredSoapActionRegexStrings = value;

                if (this.requiredSoapActionRegexStrings != null && this.requiredSoapActionRegexStrings.Length > 0)
                {
                    this.RequiredSoapActionRegularExpressions = this.requiredSoapActionRegexStrings.Select(s => new Regex(s)).ToArray();
                }
            }
        }

        private Regex[] RequiredSoapActionRegularExpressions { get; set; }

        public override ConditionResult Check(IDictionary<string, object> env)
        {
            string soapAction;

            if (!Tools.TryGetSoapAction(env, out soapAction))
            {
                return new ConditionResult(false);
            }

            var condition1 = this.RequiredSoapActions != null && this.RequiredSoapActions.Any(a => a.Equals(soapAction));
            if (condition1)
            {
                return new ConditionResult(true);
            }

            if (this.RequiredSoapActionRegularExpressions != null && this.RequiredSoapActionRegularExpressions.Length > 0)
            {
                var condition2 = this.RequiredSoapActionRegularExpressions.Any(e => e.IsMatch(soapAction));
                return new ConditionResult(condition2);
            }

            return new ConditionResult(false);
        }
    }
}
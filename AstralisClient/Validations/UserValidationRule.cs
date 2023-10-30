using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Astralis.Properties;

namespace Astralis.Validations
{
    public partial class UserValidationRule : ValidationRule
    {
        private const string userStringRegex = "^[A-Za-z0-9!@#$%&]{2,30}$";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            Regex userRegex = new Regex(userStringRegex);
            ValidationResult result = ValidationResult.ValidResult;

            string input = value.ToString();

            if (!userRegex.IsMatch(input))
            {
                result = new ValidationResult(false, Resources.tpNicknameHelp);

                if(input.Length < 2 || input.Length >30)
                {
                    result = new ValidationResult(false, Resources.tpNicknameLenghtHelp);
                }
            }

            return result;
        }
    }

    public partial class MailValidationRule : ValidationRule
    {
        private const string mailStringRegex = "^.+@[^\\.].*\\.[a-z]{2,}$"; 

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            Regex mailRegex = new Regex(mailStringRegex);
            ValidationResult result = ValidationResult.ValidResult;

            string input = value.ToString();

            if (!mailRegex.IsMatch(input))
            {
                result = new ValidationResult(false, Resources.tpMailHelp);

                if (input.Length < 2 || input.Length > 30)
                {
                    result = new ValidationResult(false, Resources.tpNicknameLenghtHelp);
                }
            }

            return result;
        }
    }
}

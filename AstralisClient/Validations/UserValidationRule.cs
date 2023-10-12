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
    public class UserValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            Regex userRegex = new Regex("^[A-Za-z0-9!@#$%&]{2,30}$");
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
}

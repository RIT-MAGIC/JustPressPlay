/*
 * Copyright 2014 Rochester Institute of Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace JustPressPlay.Filters
{
    /// <summary>
    /// Validates text line input based on a particular pattern
    /// </summary>
    public class CharacterValidatorAttribute : RegularExpressionAttribute, IClientValidatable
    {
        // Allow new line characters?
        private bool _allowNewLine;

        /// <summary>
        /// Constructor for the attribute - sets up the error message
        /// </summary>
        public CharacterValidatorAttribute(bool allowNewLine = false)
            : base(allowNewLine ? Utilities.JPPConstants.INPUT_VALID_TEXT_AREA_REGEX : Utilities.JPPConstants.INPUT_VALID_TEXT_REGEX)
        {
            _allowNewLine = allowNewLine;
            this.ErrorMessage = "The {0} field contains one or more invalid characters.";
        }

        /// <summary>
        /// Gets the validation rules for this attribute
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<ModelClientValidationRule>
          GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            // Create the rule
            var rule = new ModelClientValidationRule()
            {
                ErrorMessage = "The {0} field contains one or more invalid characters.",
                ValidationType = "textlineinput"
            };

            // Add the pattern to the rule and return
            rule.ValidationParameters.Add("pattern", _allowNewLine ? Utilities.JPPConstants.INPUT_VALID_TEXT_AREA_REGEX : Utilities.JPPConstants.INPUT_VALID_TEXT_REGEX);
            return new List<ModelClientValidationRule>() { rule };
        }
    }
}
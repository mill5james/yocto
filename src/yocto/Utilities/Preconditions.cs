﻿using System;

namespace yocto
{
    public static class Preconditions
    {
        private static void CheckParamName(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new ArgumentException($@"{nameof(paramName)} cannot be null, empty or whitespace.", nameof(paramName));
        }

        public static void CheckIsNotNull(string paramName, object value)
        {
            CheckParamName(paramName);

            if (value == null)
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");
        }

        public static void CheckIsNotNullEmptyOrWhitespace(string paramName, string value)
        {
            CheckParamName(paramName);

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null, empty or whitespace.");
        }
    }
}
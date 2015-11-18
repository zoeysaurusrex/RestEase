﻿namespace RestEase
{
    /// <summary>
    /// Type of serialization that should be applied to the body
    /// </summary>
    public enum BodySerializationMethod
    {
        /// <summary>
        /// Serialized using the configured IRequestBodySerializer (uses Json.NET by default)
        /// </summary>
        Serialized,

        /// <summary>
        /// Serialized using Form URL Encoding. The body must implement IDictionary
        /// </summary>
        UrlEncoded,
    }
}

﻿//---------------------------------------------------------------------
// <copyright file="ODataPayloadKindDetectionInfo.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Core
{
    #region Namespaces

    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Microsoft.OData.Edm;
    #endregion Namespaces

    /// <summary>
    /// Represents the set of information available for payload kind detection.
    /// </summary>
    /// <remarks>This class is used to represent the input to run payload kind detection using
    /// <see cref="ODataMessageReader.DetectPayloadKind"/>. See the documentation of that method for more 
    /// information.</remarks>
    internal sealed class ODataPayloadKindDetectionInfo
    {
        /// <summary>The parsed content type as <see cref="ODataMediaType"/>.</summary>
        private readonly ODataMediaType contentType;

        /// <summary>The encoding specified in the charset parameter of contentType or the default encoding from MediaType.</summary>
        private readonly Encoding encoding;

        /// <summary>The <see cref="ODataMessageReaderSettings"/> being used for reading the message.</summary>
        private readonly ODataMessageReaderSettings messageReaderSettings;

        /// <summary>The <see cref="IEdmModel"/> for the payload.</summary>
        private readonly IEdmModel model;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contentType">The parsed content type as <see cref="ODataMediaType"/>.</param>
        /// <param name="encoding">The encoding from the content type or the default encoding from <see cref="ODataMediaType" />.</param>
        /// <param name="messageReaderSettings">The <see cref="ODataMessageReaderSettings"/> being used for reading the message.</param>
        /// <param name="model">The <see cref="IEdmModel"/> for the payload.</param>
        internal ODataPayloadKindDetectionInfo(
            ODataMediaType contentType,
            Encoding encoding,
            ODataMessageReaderSettings messageReaderSettings, 
            IEdmModel model)
        {
            ExceptionUtils.CheckArgumentNotNull(contentType, "contentType");
            ExceptionUtils.CheckArgumentNotNull(messageReaderSettings, "readerSettings");

            this.contentType = contentType;
            this.encoding = encoding;
            this.messageReaderSettings = messageReaderSettings;
            this.model = model;
        }

        /// <summary>
        /// The <see cref="ODataMessageReaderSettings"/> being used for reading the message.
        /// </summary>
        public ODataMessageReaderSettings MessageReaderSettings
        {
            get { return this.messageReaderSettings; }
        }

        /// <summary>
        /// The <see cref="IEdmModel"/> for the payload.
        /// </summary>
        public IEdmModel Model
        {
            get { return this.model; }
        }

        /// <summary>
        /// The <see cref="ODataMessageReaderSettings"/> being used for reading the message.
        /// </summary>
        internal ODataMediaType ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        /// <summary>
        /// The encoding derived from the content type or the default encoding.
        /// </summary>
        /// <returns>The encoding derived from the content type or the default encoding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "There is computation needed to get the encoding from the content type; thus a method.")]
        public Encoding GetEncoding()
        {
            return this.encoding ?? this.contentType.SelectEncoding();
        }
    }
}

//---------------------------------------------------------------------
// <copyright file="ODataJsonLightCollectionWriter.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Core.JsonLight
{
    #region Namespaces
    using System.Diagnostics;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;

    #endregion Namespaces

    /// <summary>
    /// ODataCollectionWriter for the JsonLight format.
    /// </summary>
    internal sealed class ODataJsonLightCollectionWriter : ODataCollectionWriterCore
    {
        /// <summary>
        /// The output context to write to.
        /// </summary>
        private readonly ODataJsonLightOutputContext jsonLightOutputContext;

        /// <summary>
        /// The JsonLight collection serializer to use.
        /// </summary>
        private readonly ODataJsonLightCollectionSerializer jsonLightCollectionSerializer;

        /// <summary>
        /// Constructor for creating a collection writer to use when writing operation result payloads.
        /// </summary>
        /// <param name="jsonLightOutputContext">The output context to write to.</param>
        /// <param name="itemTypeReference">The item type of the collection being written or null if no metadata is available.</param>
        internal ODataJsonLightCollectionWriter(ODataJsonLightOutputContext jsonLightOutputContext, IEdmTypeReference itemTypeReference)
            : base(jsonLightOutputContext, itemTypeReference)
        {
            Debug.Assert(jsonLightOutputContext != null, "jsonLightOutputContext != null");

            this.jsonLightOutputContext = jsonLightOutputContext;
            this.jsonLightCollectionSerializer = new ODataJsonLightCollectionSerializer(this.jsonLightOutputContext, /*writingTopLevelCollection*/true);
        }

        /// <summary>
        /// Constructor for creating a collection writer to use when writing parameter payloads.
        /// </summary>
        /// <param name="jsonLightOutputContext">The output context to write to.</param>
        /// <param name="expectedItemType">The type reference of the expected item type or null if no expected item type exists.</param>
        /// <param name="listener">If not null, the writer will notify the implementer of the interface of relevant state changes in the writer.</param>
        internal ODataJsonLightCollectionWriter(ODataJsonLightOutputContext jsonLightOutputContext, IEdmTypeReference expectedItemType, IODataReaderWriterListener listener)
            : base(jsonLightOutputContext, expectedItemType, listener)
        {
            Debug.Assert(jsonLightOutputContext != null, "jsonLightOutputContext != null");
            Debug.Assert(!jsonLightOutputContext.WritingResponse, "The collection writer constructor for parameter payloads must only be used for writing requests.");

            this.jsonLightOutputContext = jsonLightOutputContext;
            this.jsonLightCollectionSerializer = new ODataJsonLightCollectionSerializer(this.jsonLightOutputContext, /*writingTopLevelCollection*/false);
        }

        /// <summary>
        /// Check if the object has been disposed; called from all public API methods. Throws an ObjectDisposedException if the object
        /// has already been disposed.
        /// </summary>
        protected override void VerifyNotDisposed()
        {
            this.jsonLightOutputContext.VerifyNotDisposed();
        }

        /// <summary>
        /// Flush the output.
        /// </summary>
        protected override void FlushSynchronously()
        {
            this.jsonLightOutputContext.Flush();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Flush the output.
        /// </summary>
        /// <returns>Task representing the pending flush operation.</returns>
        protected override Task FlushAsynchronously()
        {
            return this.jsonLightOutputContext.FlushAsync();
        }
#endif

        /// <summary>
        /// Start writing an OData payload.
        /// </summary>
        protected override void StartPayload()
        {
            this.jsonLightCollectionSerializer.WritePayloadStart();
        }

        /// <summary>
        /// Finish writing an OData payload.
        /// </summary>
        protected override void EndPayload()
        {
            this.jsonLightCollectionSerializer.WritePayloadEnd();
        }

        /// <summary>
        /// Start writing a collection.
        /// </summary>
        /// <param name="collectionStart">The <see cref="ODataCollectionStart"/> representing the collection.</param>
        protected override void StartCollection(ODataCollectionStart collectionStart)
        {
            this.jsonLightCollectionSerializer.WriteCollectionStart(collectionStart, this.ItemTypeReference);
        }

        /// <summary>
        /// Finish writing a collection.
        /// </summary>
        protected override void EndCollection()
        {
            this.jsonLightCollectionSerializer.WriteCollectionEnd();
        }

        /// <summary>
        /// Writes a collection item (either primitive or complex)
        /// </summary>
        /// <param name="item">The collection item to write.</param>
        /// <param name="expectedItemType">The expected type of the collection item or null if no expected item type exists.</param>
        protected override void WriteCollectionItem(object item, IEdmTypeReference expectedItemType)
        {
            if (item == null)
            {
                ValidationUtils.ValidateNullCollectionItem(expectedItemType, this.jsonLightOutputContext.MessageWriterSettings.WriterBehavior);
                this.jsonLightOutputContext.JsonWriter.WriteValue((string)null);
            }
            else
            {
                ODataComplexValue complexValue = item as ODataComplexValue;
                ODataEnumValue enumVal = null;
                if (complexValue != null)
                {
                    this.jsonLightCollectionSerializer.AssertRecursionDepthIsZero();
                    this.jsonLightCollectionSerializer.WriteComplexValue(
                        complexValue,
                        expectedItemType,
                        false /*isTopLevel*/,
                        false /*isOpenPropertyType*/,
                        this.DuplicatePropertyNamesChecker);
                    this.jsonLightCollectionSerializer.AssertRecursionDepthIsZero();
                    this.DuplicatePropertyNamesChecker.Clear();
                }
                else if ((enumVal = item as ODataEnumValue) != null)
                {
                    if (enumVal.Value == null)
                    {
                        this.jsonLightCollectionSerializer.WriteNullValue();
                    }
                    else
                    {
                        // write ODataEnumValue.Value as string value
                        this.jsonLightCollectionSerializer.WritePrimitiveValue(enumVal.Value, EdmCoreModel.Instance.GetString(true));
                    }
                }
                else
                {
                    Debug.Assert(!(item is ODataCollectionValue), "!(item is ODataCollectionValue)");
                    Debug.Assert(!(item is ODataStreamReferenceValue), "!(item is ODataStreamReferenceValue)");
                    this.jsonLightCollectionSerializer.WritePrimitiveValue(item, expectedItemType);
                }
            }
        }
    }
}

﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.OpenApiSpecification.Core.Models;
using Microsoft.OpenApiSpecification.Core.Serialization;
using Microsoft.OpenApiSpecification.Generation.Models;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApiSpecification.Generation.Tests.DocumentVariantTests
{
    public class DocumentVariantTest
    {
        private const string TestFilesDirectory = "DocumentVariantTests/TestFiles";
        private const string TestValidationDirectory = "DocumentVariantTests/TestValidation";

        private readonly ITestOutputHelper _output;

        public DocumentVariantTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GenerateV3DocumentMultipleVariantsShouldSucceed()
        {
            // Arrange
            var path = Path.Combine(TestFilesDirectory, "Annotation.xml");
            var document = XDocument.Load(path);

            var generator = new OpenApiDocumentGenerator();

            var variantInfoDefault = DocumentVariantInfo.Default;

            var variantInfo1 = new DocumentVariantInfo
            {
                Categorizer = "swagger",
                Title = "Group1"
            };

            var variantInfo2 = new DocumentVariantInfo
            {
                Categorizer = "swagger",
                Title = "Group2"
            };

            // Act
            var result = generator.GenerateV3Documents(
                document,
                new List<string> {Path.Combine(TestFilesDirectory, "OpenApiSpecification.UnitTestSamples.DotNetFrameworkController.dll")});

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GenerationStatus.Success, result.GenerationStatus);

            Assert.Equal(3, result.Documents.Keys.Count);
            Assert.Equal(7, result.PathGenerationResults.Count);

            // Default Document Variant
            VerifyDocumentVariantAgainstJsonFile(
                result,
                variantInfoDefault,
                Path.Combine(TestValidationDirectory, "AnnotationDefaultVariant.json"));

            // Document Variant 1
            // Only contains info from operations with tags with title "Group1" in Annotation.xml 
            VerifyDocumentVariantAgainstJsonFile(
                result,
                variantInfo1,
                Path.Combine(TestValidationDirectory, "AnnotationVariant1.json"));

            // Document Variant 2
            // Only contains info from operations with tags with title "Group2" in Annotation.xml 
            VerifyDocumentVariantAgainstJsonFile(
                result,
                variantInfo2,
                Path.Combine(TestValidationDirectory, "AnnotationVariant2.json"));
        }

        private void VerifyDocumentVariantAgainstJsonFile(
            DocumentGenerationResult result,
            DocumentVariantInfo documentVariantInfo,
            string jsonFileName)
        {
            OpenApiV3SpecificationDocument specificationDocument;

            result.Documents.TryGetValue(documentVariantInfo, out specificationDocument);
            Assert.NotNull(specificationDocument);

            var actualDocument = JsonConvert.SerializeObject(
                specificationDocument,
                new JsonSerializerSettings {ContractResolver = new EmptyCollectionContractResolver()}
            );

            var expectedDocument = File.ReadAllText(jsonFileName);

            Assert.True(TestHelper.AreJsonEqual(expectedDocument, actualDocument));
        }
    }
}
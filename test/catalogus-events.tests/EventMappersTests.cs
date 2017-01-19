using catalogus_events.Events;
using catalogus_events.Mapping;
using catalogus_events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace catalogus_events.tests
{
    public class EventMapperTests
    {
        [Fact]
        public void ValidateMapping()
        {
            var mapper = EventMappers.CreateMapper();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void MapProductToArtikelAanCatalogusToegevoegd()
        {
            var product = new Product
            {
                Naam = "HL Road Frame - Black, 58",
                Beschrijving = "Our lightest and best quality aluminum frame made from the newest alloy; it is welded and heat-treated for strength.Our innovative design results in maximum comfort and performance.",
                AfbeeldingUrl = "no_image_available_small.gif",
                Prijs = 1431.50m,
                LeverbaarVanaf = new DateTime(1998, 06, 01),
                LeverbaarTot = null,
                LeveranciersProductCode = "FR - R92B - 58",
                Categorieen = new List<ProductCategorie>
                {
                    new ProductCategorie
                    {
                        Categorie = new Categorie
                        {
                            Naam = "Clothing"
                        }
                    }
                },
                Leverancier = new Leverancier
                {
                    Naam = "Koga Miyata"
                }
            };

            var mapper = EventMappers.CreateMapper();

            var result = mapper.Map<ArtikelAanCatalogusToegevoegd>(product);
            Assert.Equal(new string[] { "Clothing" }, result.Categorieen);

            Assert.Equal("Koga Miyata", result.Leverancier);
        }

        [Fact]
        public void IncludeImageInArtikelAanCatalogusToegevoegdEvent()
        {
            var product = new Product
            {
                AfbeeldingUrl = "awc_jersey_male_small.gif"
            };

            var mapper = EventMappers.CreateMapper();
            var message = mapper.Map<ArtikelAanCatalogusToegevoegd>(product);

            Assert.NotNull(message.Afbeelding);
            Assert.NotEmpty(message.Afbeelding);
        }

        [Fact]
        public void EmptyAfbeeldingWhenFileNotExists()
        {
            var product = new Product
            {
                AfbeeldingUrl = "something_that_is_definitely_not_there.gif"
            };

            var mapper = EventMappers.CreateMapper();
            var message = mapper.Map<ArtikelAanCatalogusToegevoegd>(product);

            Assert.Null(message.Afbeelding);
        }
    }
}

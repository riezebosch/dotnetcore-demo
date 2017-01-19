using AutoMapper;
using catalogus_events.Events;
using catalogus_events.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace catalogus_events.Mapping
{
    public static class EventMappers
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ArtikelAanCatalogusToegevoegd>(MemberList.Source)
                    .ForSourceMember(src => src.LeverancierId, opt => opt.Ignore())
                    .ForMember(dest => dest.Afbeelding, opt => opt.MapFrom(p => ReadImage(p.AfbeeldingUrl)))
                    .ForMember(dest => dest.LeverancierCode, opt => opt.MapFrom(p => p.LeveranciersProductCode))
                    .ForMember(dest => dest.Leverancier, opt => opt.MapFrom(p => p.Leverancier.Naam))
                    .ForMember(dest => dest.Categorieen, opt => opt.MapFrom(p => p.Categorieen.Select(pc => pc.Categorie.Naam)));
            });

            return config.CreateMapper();
        }

        private static byte[] ReadImage(string url)
        {
            var path = Path.Combine("Plaatjes", url);
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            return null;
        }
    }
}

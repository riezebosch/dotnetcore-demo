using AutoMapper;
using catalogus_events.Events;
using catalogus_events.Model;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace catalogus_events
{
    public class ProductPublisher
    {
        private IMapper _mapper;

        public ProductPublisher(IMapper mapper)
        {
            _mapper = mapper;
        }

        public void Publish(ISender sender, IEnumerable<Product> producten)
        {
            foreach (var p in producten)
            {
                sender.PublishEvent(_mapper.Map<ArtikelAanCatalogusToegevoegd>(p));
            }
        }
    }
}

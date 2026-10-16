using AutoMapper;
using LibraryAPI.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibraryAPI.Tests
{
    public class MappingFixture
    {
        public IMapper mapper { get; }

        public MappingFixture()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
            mapper = config.CreateMapper();
        }
    }
}

using System;
using System.Collections.Generic;

namespace NureSEConsultations.Bot.Model
{
    public class CachingRepository : IConsultationRepository
    {
        private const int CACHE_DURATION_SECONDS = 60 * 60; 

        private readonly ConsultationRepository consultationRepository;

        private readonly IDictionary<string, CacheEntry> cache;

        public CachingRepository(ConsultationRepository consultationRepository)
        {
            this.consultationRepository = consultationRepository;
            this.cache = new Dictionary<string, CacheEntry>();
        }

        public IEnumerable<Consultation> GetAllByType(string type)
        {
            if (this.cache.TryGetValue(type, out CacheEntry cacheEntry)
                && DateTime.Now <= cacheEntry.ValidUntil)
            {
                return cacheEntry.Consultations;
            }

            var retrievedValue = this.consultationRepository.GetAllByType(type);
            this.cache[type] = new CacheEntry(
                retrievedValue, 
                DateTime.Now.AddSeconds(CACHE_DURATION_SECONDS));
            return retrievedValue;
        }

        public IEnumerable<string> GetConsultationsNames()
        {
            return this.consultationRepository.GetConsultationsNames();
        }

        private record CacheEntry(
            IEnumerable<Consultation> Consultations,
            DateTime ValidUntil
        );
    }
}

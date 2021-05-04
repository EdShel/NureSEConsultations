using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NureSEConsultations.Bot.Model;
using System.Collections.Generic;
using System.Linq;
using Version = Lucene.Net.Util.Version;

namespace NureSEConsultations.Bot.Services
{
    public interface IConsultationSearcher
    {
        IEnumerable<Consultation> Search(string text);
    }

    public class ConsultationSearcher : IConsultationSearcher
    {
        private const int MAX_RESULTS = 100;

        private readonly Directory searchIndexesDirectory;

        private readonly Analyzer searchAnalyzer;

        private readonly IConsultationRepository consultationRepository;

        public ConsultationSearcher(IConsultationRepository consultationRepository)
        {
            this.consultationRepository = consultationRepository;

            this.searchIndexesDirectory = new RAMDirectory();
            this.searchAnalyzer = new StandardAnalyzer(Version.LUCENE_30);

            BuildSearchIndices();
        }

        public IEnumerable<Consultation> Search(string text)
        {
            using var indexSearcher = new IndexSearcher(this.searchIndexesDirectory);

            var searchFields = new[] { nameof(Consultation.Teacher), nameof(Consultation.Subject), nameof(Consultation.Group) };
            var queryParser = new MultiFieldQueryParser(Version.LUCENE_30, searchFields, this.searchAnalyzer);

            var query = queryParser.Parse(text);
            var hits = indexSearcher.Search(query, MAX_RESULTS);

            foreach (var scoreDoc in hits.ScoreDocs)
            {
                Document d = indexSearcher.Doc(scoreDoc.Doc);
                yield return new Consultation
                {
                    Group = d.Get(nameof(Consultation.Group)),
                    Link = d.Get(nameof(Consultation.Link)),
                    Subject = d.Get(nameof(Consultation.Subject)),
                    Teacher = d.Get(nameof(Consultation.Teacher)),
                    Time = d.Get(nameof(Consultation.Time))
                };
            }
        }

        private void BuildSearchIndices()
        {
            using var indexWriter = new IndexWriter(
                d: this.searchIndexesDirectory,
                a: this.searchAnalyzer,
                create: true,
                mfl: IndexWriter.MaxFieldLength.UNLIMITED
            );

            var allConsultations = this.consultationRepository.GetConsultationsNames()
                .SelectMany(type => this.consultationRepository.GetAllByType(type));

            foreach (var consultation in allConsultations)
            {
                var d = new Document();
                d.Add(new Field(nameof(Consultation.Subject), consultation.Subject, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
                d.Add(new Field(nameof(Consultation.Teacher), consultation.Teacher, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
                d.Add(new Field(nameof(Consultation.Group), consultation.Group, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
                d.Add(new Field(nameof(Consultation.Time), consultation.Time, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.YES));
                d.Add(new Field(nameof(Consultation.Link), consultation.Link, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.YES));

                indexWriter.AddDocument(d);
            }
            indexWriter.Optimize();
        }
    }
}

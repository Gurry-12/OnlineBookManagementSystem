using System.Linq;
using System.Threading.Tasks;
using DnsClient;

namespace OnlineBookManagementSystem.Helper
{
     class DNSCheckerHelper : IDnsChecker
    {
        public async Task<bool> DomainHasMxRecordAsync(string email)
        {
            var domain = email.Split('@').LastOrDefault();
            if (string.IsNullOrEmpty(domain)) return false;

            try
            {
                var lookup = new LookupClient();
                var result = await lookup.QueryAsync(domain, QueryType.MX);
                return result.Answers.MxRecords().Any();
            }
            catch
            {
                return false;
            }
        }
    }
}

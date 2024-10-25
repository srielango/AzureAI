using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.AzureAILanguage.Queries.AnalyzeText;

public record AnalyzeTextQuery : IRequest<string>
{
}

public class AnalyzeTextQueryValidator : AbstractValidator<AnalyzeTextQuery>
{
    public AnalyzeTextQueryValidator()
    {
    }
}

public class AnalyzeTextQueryHandler : IRequestHandler<AnalyzeTextQuery, string>
{
    private readonly IApplicationDbContext _context;

    public AnalyzeTextQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(AnalyzeTextQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

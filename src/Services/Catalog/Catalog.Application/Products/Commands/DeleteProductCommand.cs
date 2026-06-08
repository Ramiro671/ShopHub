using MediatR;

namespace Catalog.Application.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

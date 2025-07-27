
using FluentValidation;
using QuiosqueFood3000.Kitchen.Domain.Entities;

namespace QuiosqueFood3000.Kitchen.Application.Validators;

public class OrderSolicitationValidator : AbstractValidator<OrderSolicitation>
{
    public OrderSolicitationValidator()
    {
        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("Order must contain at least one product.");

        RuleForEach(x => x.Products).ChildRules(product =>
        {
            product.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name cannot be empty.");

            product.RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description cannot be empty.");
        });
    }
}

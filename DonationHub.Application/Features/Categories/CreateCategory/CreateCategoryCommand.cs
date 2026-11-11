using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Domain.Categories;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Categories.CreateCategory
{
    public sealed record CreateCategoryCommand : IRequest<Result<CategoryDto>>
    {
        public string NameAr { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
        public string? DescriptionAr { get; init; }
        public string? DescriptionEn { get; init; }
    }

    public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
            RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DescriptionAr).MaximumLength(1000);
            RuleFor(x => x.DescriptionEn).MaximumLength(1000);
        }
    }

    public sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork uow, TimeProvider timeProvider) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var nameAr = request.NameAr.Trim();
            var nameEn = request.NameEn.Trim();
            if(await categoryRepository.NameExistsAsync(nameAr, nameEn,null,cancellationToken))
                return CategoryErrors.NameAlreadyExists();
            var category = Category.Create(request.NameAr,request.NameEn, request.DescriptionAr ?? "", request.DescriptionEn ?? "", timeProvider.GetUtcNow().UtcDateTime);
            await categoryRepository.AddAsync(category, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            return CategoryDto.From(category);


        }
    }
}

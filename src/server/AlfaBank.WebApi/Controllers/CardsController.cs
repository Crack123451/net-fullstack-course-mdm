// <copyright file="CardsController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AlfaBank.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using AlfaBank.Core.Data.Interfaces;
    using AlfaBank.Core.Extensions;
    using AlfaBank.Core.Infrastructure;
    using AlfaBank.Core.Models;
    using AlfaBank.Core.Models.Dto;
    using AlfaBank.Core.Models.Factories;
    using AlfaBank.Services.Checkers;
    using AlfaBank.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Class for work with Cards.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [BindProperties]
    public class CardsController : ControllerBase
    {

        private readonly IUserRepository userRepository;
        private readonly ICardRepository cardRepository;
        private readonly ICardChecker cardChecker;
        private readonly IDtoValidationService dtoValidationService;
        private readonly IBankService bankService;
        private readonly IDtoFactory<Card, CardGetDto> dtoFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardsController"/> class.
        /// Constructor class for work with Cards.
        /// </summary>
        /// <param name="dtoValidationService">Validation dto Service.</param>
        /// <param name="cardRepository">Repository card.</param>
        /// <param name="userRepository">Repository user.</param>
        /// <param name="cardChecker">"Checker card.</param>
        /// <param name="bankService">Service bank.</param>
        /// <param name="dtoFactory">Factory dto.</param>
        [ExcludeFromCodeCoverage]
        public CardsController(
            IDtoValidationService dtoValidationService,
            ICardRepository cardRepository,
            IUserRepository userRepository,
            ICardChecker cardChecker,
            IBankService bankService,
            IDtoFactory<Card, CardGetDto> dtoFactory)
        {
            this.dtoValidationService = dtoValidationService ??
                                    throw new ArgumentNullException(nameof(dtoValidationService));
            this.cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.cardChecker = cardChecker ?? throw new ArgumentNullException(nameof(cardChecker));
            this.bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
            this.dtoFactory = dtoFactory ?? throw new ArgumentNullException(nameof(dtoFactory));
        }

        /// <summary>
        /// Method Get.
        /// </summary>
        /// <returns><see cref="CardGetDto"/> list.</returns>
        // GET api/cards
        [HttpGet]
        public ActionResult<IEnumerable<CardGetDto>> Get()
        {
            // Select
            var cards = this.cardRepository.All(this.userRepository.GetCurrentUser());

            // Mapping
            var cardsDto = this.dtoFactory.Map(cards, this.TryValidateModel);

            // Return
            return this.Ok(cardsDto);
        }

        /// <summary>
        /// Method Get.
        /// </summary>
        /// <param name="number">Number credit card.</param>
        /// <returns><see cref="CardGetDto"/> class.</returns>
        // GET api/cards/5334343434343...
        [HttpGet("{number}")]
        public ActionResult<CardGetDto> Get([CreditCard] string number)
        {
            // Validate
            if (!this.cardChecker.CheckCardEmitter(number))
            {
                this.ModelState.AddModelError("number", "This card number is invalid");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select
            var card = this.cardRepository.Get(this.userRepository.GetCurrentUser(), number);

            // Mapping
            var dto = this.dtoFactory.Map(card, this.TryValidateModel);

            // Validate
            if (dto == null)
            {
                return this.NotFound();
            }

            return this.Ok(dto);
        }

        /// <summary>
        /// Method Post.
        /// </summary>
        /// <param name="value">Card dto.</param>
        /// <returns><see cref="CardGetDto"/> class.</returns>
        // POST api/cards
        [HttpPost]
        public ActionResult<CardGetDto> Post([FromBody] CardPostDto value)
        {
            // Validate
            var validateResult = this.dtoValidationService.ValidateOpenCardDto(value);
            if (validateResult.HasErrors())
            {
                this.ModelState.AddErrors(validateResult);
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select
            var (card, openResult) = this.bankService.TryOpenNewCard(
                this.userRepository.GetCurrentUser(),
                value.Name,
                (Currency) value.Currency,
                (CardType) value.Type);

            if (openResult.HasErrors())
            {
                this.ModelState.AddErrors(openResult);
                return this.BadRequest(this.ModelState);
            }

            // Mapping
            var dto = this.dtoFactory.Map(card, this.TryValidateModel);

            // Validate
            if (dto == null)
            {
                return this.BadRequest("Не удалось выпустить карту");
            }

            return this.Created($"/api/cards/{dto.Number}", dto);
        }

        /// <summary>
        /// Method Delete.
        /// </summary>
        /// <returns><see cref="IActionResult"/>Status code.</returns>
        // DELETE api/cards
        [HttpDelete]
        public IActionResult Delete() => this.StatusCode(405);

        /// <summary>
        /// Method Put.
        /// </summary>
        /// <returns><see cref="IActionResult"/>Status code.</returns>
        // PUT api/cards
        [HttpPut]
        public IActionResult Put() => this.StatusCode(405);
    }
}
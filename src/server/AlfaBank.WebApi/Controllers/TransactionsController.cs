// <copyright file="TransactionsController.cs" company="PlaceholderCompany">
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
    using AlfaBank.Core.Models;
    using AlfaBank.Core.Models.Dto;
    using AlfaBank.Core.Models.Factories;
    using AlfaBank.Services.Checkers;
    using AlfaBank.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Class for work with Transaction.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [BindProperties]
    public class TransactionsController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IBankService bankService;
        private readonly ITransactionRepository transactionRepository;
        private readonly ICardChecker cardChecker;
        private readonly IDtoValidationService dtoValidationService;
        private readonly IDtoFactory<Transaction, TransactionGetDto> dtoFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionsController"/> class.
        /// Constructor class for work with Transaction.
        /// </summary>
        /// <param name="dtoValidationService">Service validation dto.</param>
        /// <param name="userRepository">Repository user.</param>
        /// <param name="transactionRepository">Repository transaction.</param>
        /// <param name="cardChecker">Checker card.</param>
        /// <param name="bankService">Service bank.</param>
        /// <param name="dtoFactory">Factory dto.</param>
        [ExcludeFromCodeCoverage]
        public TransactionsController(
            IDtoValidationService dtoValidationService,
            IUserRepository userRepository,
            ITransactionRepository transactionRepository,
            ICardChecker cardChecker,
            IBankService bankService,
            IDtoFactory<Transaction, TransactionGetDto> dtoFactory)
        {
            this.dtoValidationService = dtoValidationService ??
                                    throw new ArgumentNullException(nameof(dtoValidationService));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.transactionRepository =
                transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            this.cardChecker = cardChecker ?? throw new ArgumentNullException(nameof(cardChecker));
            this.bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
            this.dtoFactory = dtoFactory ?? throw new ArgumentNullException(nameof(dtoFactory));
        }

        /// <summary>
        /// Method Get.
        /// </summary>
        /// <param name="number">Number credit card.</param>
        /// <param name="skip">Count skip cards.</param>
        /// <returns><see cref="TransactionGetDto"/> list.</returns>
        // GET api/transactions/5334343434343?skip=...
        [HttpGet("{number}")]
        public ActionResult<IEnumerable<TransactionGetDto>> Get(
            [Required] [CreditCard] string number, [FromQuery] [Range(1, 1000)] int skip = 0)
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
            var transactions = this.transactionRepository.Get(
                this.userRepository.GetCurrentUser(),
                number,
                skip,
                10);

            // Mapping
            var transactionsDto = this.dtoFactory.Map(transactions, this.TryValidateModel);

            // Return
            return this.Ok(transactionsDto);
        }

        /// <summary>
        /// Method Post.
        /// </summary>
        /// <param name="value">Transaction model dto.</param>
        /// <returns><see cref="TransactionGetDto"/> class.</returns>
        // POST api/transactions
        [HttpPost]
        public ActionResult<TransactionGetDto> Post([FromBody] [Required] TransactionPostDto value)
        {
            // Validate
            var validateResult = this.dtoValidationService.ValidateTransferDto(value);
            if (validateResult.HasErrors())
            {
                this.ModelState.AddErrors(validateResult);
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Work
            var (transaction, transferResult) = this.bankService.TryTransferMoney(
                this.userRepository.GetCurrentUser(),
                value.Sum,
                value.From,
                value.To);

            if (transferResult.HasErrors())
            {
                this.ModelState.AddErrors(transferResult);
                return this.BadRequest(this.ModelState);
            }

            var cardFromNumber = value.From.ToNormalizedCardNumber();

            var dto = this.dtoFactory.Map(transaction, this.TryValidateModel);

            // Validate
            if (dto == null)
            {
                return this.BadRequest("Transferring error");
            }

            return this.Created($"/transactions/{cardFromNumber}", dto);
        }

        /// <summary>
        /// Method delete.
        /// </summary>
        /// <returns><see cref="IActionResult"/> class.</returns>
        // DELETE api/transactions
        [HttpDelete]
        public IActionResult Delete() => this.StatusCode(405);

        /// <summary>
        /// Method put.
        /// </summary>
        /// <returns><see cref="IActionResult"/> class.</returns>
        // PUT api/transactions
        [HttpPut]
        public IActionResult Put() => this.StatusCode(405);
    }
}
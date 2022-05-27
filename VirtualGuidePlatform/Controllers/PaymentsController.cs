using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Repositories;

namespace VirtualGuidePlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private const string ApiKey = "sk_test_51L0Yf4FAWZEgTFzWGsM6cXkKuvzw94h1qGSYtlMusgTYJq8wuFutxP4DjReTSdEvYVFqjPUykoVBrmaB3larMhjg00j55pEr0a";
        private IGuidesRepository _guidesRepository;
        private IPaymentsRepository _paymentsRepository;
        private IAccountsRepository _accountsRepository;

        public PaymentsController(IGuidesRepository guidesRepo, IPaymentsRepository paymentsRepository, IAccountsRepository accountsRepository)
        {
            _guidesRepository = guidesRepo;
            _paymentsRepository = paymentsRepository;
            _accountsRepository = accountsRepository;
        }

        public class PostModel
        {
            public string guideID { get; set; }
            public string userID { get; set; }
        }

        [HttpPost("create-payment-intent")] 
        public async Task<IActionResult> Index(PostModel model)
        {
            StripeConfiguration.ApiKey = ApiKey;

            var guide = await _guidesRepository.GetGuide(model.guideID);

            var options = new PaymentIntentCreateOptions
            {
                Amount = Convert.ToInt64(guide.price*100),
                Currency = "eur",
                PaymentMethodTypes = new List<string> { "card" },
                StatementDescriptor = "Buying Guide",
            };

            var service = new PaymentIntentService();
            var intent = service.Create(options);



            // Irasyti i duombaze vartotojo id ir gido id ir payment id
            // model.userID, model.guideID ir intent.id = payment id

            Payment payment = new Payment() { uID = model.userID, gID = model.guideID, pID = intent.Id };

            var paymentresp = await _paymentsRepository.CreatePayment(payment);

            return Ok(new {client_secret = intent.ClientSecret});
        }

        [HttpPost("payment-check/{payment_id}")]
        public async Task<ActionResult<AccountsDto>> CheckPayment(string payment_id)
        {
            StripeConfiguration.ApiKey = ApiKey;
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(payment_id);
            if(intent.Status.Trim() == "succeeded"){
                Console.WriteLine("Apmoketas");
                // Pagal payment id paimti userid ir guideid
                // ir irasyti gido id, useriui i buyedGuides array
                var payment = await _paymentsRepository.GetPayment(payment_id);
                if (payment != null)
                {
                    var accountUpdated = await _accountsRepository.UpdateAddPayed(payment.gID, payment.uID);

                    if (accountUpdated == null)
                    {
                        return BadRequest("Nepavyko");
                    }
                    return Ok(accountUpdated);
                }
            }
            return BadRequest("Nepavyko");
        }
    }
}

﻿

namespace Shopping_Test.Controllers
{
   /*[Authorize]*/
    public class CardController : Controller
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public CardController(
        UserManager<ApplicationUser> userManager ,
         IUnitOfWork unitOfWork)
        {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        }
        public async Task< IActionResult> Index()
        {
          
            string? IdUser = _userManager.GetUserId(User);

            var Cards = await _unitOfWork.Cards.GetAll(u => u.ApplicationUserId == IdUser && u.Favourite == true, new[] { "Products", "Marka" });
             return View(Cards);
        }
        public async Task<ActionResult> Favourite()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var favourite = await _unitOfWork.Cards.GetAll(c => c.ApplicationUserId == userEmail && c.Favourite == true);
            return View(favourite);
        }
        public async Task<ActionResult> Buy()
        {
            string? idUser = _userManager.GetUserId(User);

            var buyedCards = await _unitOfWork.Cards.GetAll(u => u.ApplicationUserId == idUser && u.Buyed == true,new[] { "Products", "Marka" });
            return View(buyedCards);
        }
        [AllowAnonymous]
        public async Task<ActionResult> addFavourite(string Id)
        {
            ///Identity/Account/Login
            var productId = Convert.ToInt32(Id);
           var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return Ok("login");
            };
            UserProducts userProducts = new UserProducts
            {
                ApplicationUserId = userId,
                ProductId = productId
            };
            var checkFavourite = await _unitOfWork.Cards.FindByCriteria(f => f.ApplicationUserId == userId && f.ProductId == productId );
            if (checkFavourite is null)
            {
                Card card = new Card
                {
                    Favourite = true,
                    Buyed = false,
                    ApplicationUserId = userId,
                    ProductId = productId,
                    mount = 1
                };
                await _unitOfWork.Cards.Add(card);
                await _unitOfWork.UserProducts.Add(userProducts);
                await _unitOfWork.Complete();
                return Ok();
            }
            else if (checkFavourite != null)
            {
                if (checkFavourite.Buyed == true && checkFavourite.Favourite == false)
                {
                    checkFavourite.Favourite = true;
                    await _unitOfWork.UserProducts.Add(userProducts);
                    await _unitOfWork.Complete();
                    return Ok();
                }
             };
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> addBuyed(addBuyed addBuyed)
        {
            string? idUser = _userManager.GetUserId(User);

            var checkBuyed = await _unitOfWork.Cards.FindByCriteria(f => f.ApplicationUserId == idUser &&  f.ProductId == addBuyed.ProductId );
            if (checkBuyed is not null)
            {
                if (checkBuyed.Favourite==true && checkBuyed.Buyed==false)
                {
                    checkBuyed.Buyed = true;
                    checkBuyed.mount = addBuyed.ValueMount;
                    await _unitOfWork.Complete();
                    return Ok();
                }
                else if(checkBuyed.Favourite == true && checkBuyed.Buyed == true)
                {
                    checkBuyed.mount = addBuyed.ValueMount;
                    await _unitOfWork.Complete();
                    return Ok();
                 }
                }
                return Ok();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var Id = Convert.ToInt32(id);
            var card = await _unitOfWork.Cards.FindByCriteria(c=> c.Id == id);
            if (card is null)
            {
                return Ok();
            }
            _unitOfWork.Cards.Remove(card);
           await _unitOfWork.Complete();
            return Ok();
        }

        public async Task<IActionResult> DeleteFavourite(int id)
        {
            var Id = Convert.ToInt32(id);
            var card = await _unitOfWork.Cards.FindByCriteria(c=> c.Id == id);
            if (card is null)
            {
                return Ok();
            }
            UserProducts userProducts =await _unitOfWork.UserProducts.FindByCriteria(u => u.ProductId == card.ProductId);
            if(card.Favourite ==true && card.Buyed == false)
            {
                if (userProducts != null)
                    _unitOfWork.UserProducts.Remove(userProducts);

                _unitOfWork.Cards.Remove(card);
            }
            else if(card.Favourite == true && card.Buyed == true)
            {
                card.Favourite = false;
                if (userProducts != null)
                    _unitOfWork.UserProducts.Remove(userProducts);

            }
            await _unitOfWork.Complete();
            return Ok();
        }
        public async Task<IActionResult> DeleteBuyed(int id)
        {
            var Id = Convert.ToInt32(id);
            var card = await _unitOfWork.Cards.FindByCriteria(c => c.Id == id);
            if (card is null)
            {
                return Ok();
            }
            card.Buyed = false;
           await _unitOfWork.Complete();
            return Ok();
        }

    }
}

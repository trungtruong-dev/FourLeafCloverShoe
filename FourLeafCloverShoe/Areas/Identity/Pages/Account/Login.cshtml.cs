﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FourLeafCloverShoe.Share.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using FourLeafCloverShoe.Services;
using FourLeafCloverShoe.IServices;
using NuGet.Configuration;

namespace FourLeafCloverShoe.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ICartService _cartService;
        private readonly ICartItemService _cartItemService;
        private readonly IProductDetailService _IProductDetailService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager, ICartService cartService, ICartItemService cartItemService, IProductDetailService productDetailService, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _cartService = cartService;
            _cartItemService = cartItemService;
            _IProductDetailService = productDetailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }


        public IList<AuthenticationScheme> ExternalLogins { get; set; }


        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }


        public class InputModel
        {

            [Required]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var username = new EmailAddressAttribute().IsValid(Input.Email) ? new MailAddress(Input.Email).User : Input.Email;
                var result = await _signInManager.PasswordSignInAsync(username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                var user = await _userManager.FindByNameAsync(username);
                var cart = await _cartService.GetByUserId(user.Id);

                // Lấy danh sách các sản phẩm trong giỏ hàng từ CSDL và phiên làm việc
                var cartItemsDb = (await _cartItemService.Gets()).Where(c => c.CartId == cart.Id).ToList();
                var cartItemsSession = SessionServices.GetCartItems(HttpContext.Session, "Cart");

                if (result.Succeeded)
                {
                    // Xử lý các sản phẩm trong phiên làm việc trước
                    foreach (var sessionItem in cartItemsSession)
                    {
                        var existingItem = cartItemsDb.FirstOrDefault(c => c.ProductDetailId == sessionItem.ProductDetailId);
                        if (existingItem == null)
                        {
                            // Thêm sản phẩm mới từ phiên làm việc vào giỏ hàng trong CSDL
                            await _cartItemService.Add(new CartItem
                            {
                                Id = Guid.NewGuid(),
                                ProductDetailId = sessionItem.ProductDetailId,
                                CartId = cart.Id,
                                Quantity = sessionItem.Quantity
                            });
                        }
                        else
                        {
                            // Cập nhật số lượng sản phẩm đã tồn tại trong giỏ hàng
                            var productDetail = await _IProductDetailService.GetById((Guid)sessionItem.ProductDetailId);
                            if (productDetail.Quantity >= sessionItem.Quantity + existingItem.Quantity)
                            {
                                existingItem.Quantity += sessionItem.Quantity;
                               await _cartItemService.Update(existingItem);
                            }

                        }
                    }
                    //Xóa các sản phẩm đã xử lý khỏi phiên làm việc
                    SessionServices.SetCartItems(HttpContext.Session, "Cart", new List<CartItem>());

                    // Lấy các vai trò của người dùng
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                    {
                        returnUrl = Url.Content("~/Admin/Statistical");
                    }
                    else if (roles.Contains("Staff"))
                    {
                        returnUrl = Url.Content("~/Admin/Statistical");
                    }
                        return LocalRedirect(returnUrl);
                    
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

    }
}

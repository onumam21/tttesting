using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;

namespace SauceDemoTests
{
    public class SauceDemoAutomatedTest
    {
        private IWebDriver driver;
        private string logPath = Path.Combine(Environment.CurrentDirectory, "test_log.txt");

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.saucedemo.com/");
            Log("Opened SauceDemo website.");
        }

        [Test]
        public void CompleteOrderTest()
        {
            // Login
            driver.FindElement(By.Id("user-name")).SendKeys("standard_user");
            driver.FindElement(By.Id("password")).SendKeys("secret_sauce");
            driver.FindElement(By.Id("login-button")).Click();
            Log("Logged in as standard_user.");

            // Add items to cart
            AddItemToCart("add-to-cart-sauce-labs-backpack");
            AddItemToCart("add-to-cart-sauce-labs-bike-light");
            AddItemToCart("add-to-cart-sauce-labs-bolt-t-shirt");

            // Remove an item
            driver.FindElement(By.Id("remove-sauce-labs-backpack")).Click();
            Log("Removed Backpack from cart.");

            // Proceed to checkout
            driver.FindElement(By.Id("shopping_cart_container")).Click();
            driver.FindElement(By.Id("checkout")).Click();
            Log("Proceeded to checkout.");

            // Enter user info and continue
            driver.FindElement(By.Id("first-name")).SendKeys("John");
            driver.FindElement(By.Id("last-name")).SendKeys("Doe");
            driver.FindElement(By.Id("postal-code")).SendKeys("12345");
            driver.FindElement(By.Id("continue")).Click();
            Log("Entered user information and continued.");

            // Verify total with tax and finish order
            double total = CalculateTotalPrice();
            double tax = total * 0.08; // Assume 8% tax rate
            double totalWithTax = total + tax;

            Assert.That(Math.Round(GetDisplayedTotal(), 2), Is.EqualTo(Math.Round(totalWithTax, 2)), "Total price with tax mismatch.");
            driver.FindElement(By.Id("finish")).Click();
            Log("Order completed.");

            // Verify thank you message
            Assert.IsTrue(driver.PageSource.Contains("Thank you for your order!"), "Order confirmation message not found.");
            Log("Verified order confirmation message.");
        }

        private void AddItemToCart(string itemId)
        {
            driver.FindElement(By.Id(itemId)).Click();
            Log($"Added item with ID '{itemId}' to cart.");
        }

        private double CalculateTotalPrice()
        {
            var priceElements = driver.FindElements(By.ClassName("inventory_item_price"));
            double totalPrice = 0.0;
            foreach (var priceElement in priceElements)
            {
                double price = double.Parse(priceElement.Text.Replace("$", ""));
                totalPrice += price;
            }
            Log($"Calculated total price: ${totalPrice}");
            return totalPrice;
        }

        private double GetDisplayedTotal()
        {
            var totalElement = driver.FindElement(By.ClassName("summary_total_label"));
            double displayedTotal = double.Parse(totalElement.Text.Replace("Total: $", ""));
            Log($"Displayed total price with tax: ${displayedTotal}");
            return displayedTotal;
        }

        private void Log(string message)
        {
            File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
                Log("Closed browser and disposed driver.");
            }
        }
    }
}

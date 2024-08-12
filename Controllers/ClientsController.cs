using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Clients.Models;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web;

namespace Clients.Controllers
{
    public class ClientsController : Controller
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://csadms.com/Devbox/DevboxAPI/")
        };
        private IEnumerable<object> homePageCounter;

        public object CommonHeader { get; private set; }
        public string Submit { get; private set; }
        public object ImagePath { get; private set; }

        [HttpGet]
        public async Task<ActionResult> HomePageCounters()
        {
            try
            {
                
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewHomePageCounterList", new object());

                if (responseMessage.IsSuccessStatusCode)
                {
                   
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseData);

                  
                    var homeClientsList = apiResponse?.Data ?? new List<HomeClients>();

                   
                    return View(homeClientsList);
                }
                else
                {
                   
                    ViewBag.ErrorMessage = $"API call failed with status code {responseMessage.StatusCode}";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
              
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("Error"); 
            }
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            return View();
        }


      [HttpPost]
        
        public async Task<ActionResult> Create(HomeClients model, HttpPostedFileBase file)
        {
           //if (ModelState.IsValid)
           // {
             string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";
                HomeClients obj = new HomeClients();
           // Class obj=new Class();
            obj.FlagId = 1;

                using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
                {
                    // Handle file upload
                    if (file != null && file.ContentLength > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.InputStream.CopyToAsync(memoryStream);
                            byte[] fileBytes = memoryStream.ToArray();
                            string base64FileData = Convert.ToBase64String(fileBytes);

                            model.ImagePath = file.FileName;
                            model.divImagePath = base64FileData; 
                        }
                    }
                    else
                    {
                        model.ImagePath = string.Empty;
                        model.divImagePath = string.Empty;
                    }

                    
                    HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewAddHomePageCounter", obj);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = await responseMessage.Content.ReadAsStringAsync();
                        var response = JObject.Parse(responseData);
                        bool isStatus = Convert.ToBoolean(response.SelectToken("Status"));
                        string message = response.SelectToken("Message").ToString();

                        TempData["Success"] = message;

                        return RedirectToAction("HomePageCounters");
                    }
                    else
                    {
                        // Handle HTTP error response
                        ModelState.AddModelError("", "An error occurred while creating the item.");
                    }
                }
            

            // If we got this far, something failed, redisplay the form
            return View(model);
        }
     
  [HttpGet]
        public async Task<ActionResult> EditHomePageCounters(int id)
        {
            string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";

            using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
            {
                HomeClients obj = new HomeClients();
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewHomePageCounterList", new { Id = id });

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return new HttpStatusCodeResult((int)responseMessage.StatusCode);
                }

                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var Response = JObject.Parse(responseData);
                var isData = Response.SelectToken("Data").ToString();

                var homePageCounter = JsonConvert.DeserializeObject<List<HomeClients>>(isData);

                if (homePageCounter == null)
                {
                    return NotFound();
                }
                List<HomeClients> objLists = new List<HomeClients>();
                foreach (var listItems in homePageCounter)
                {
                    HomeClients objs = new HomeClients();

                    objs.TextEn = listItems.TextEn;
                    TempData["TextEn"] = listItems.TextEn;
                    TempData["value"] = listItems.Value;
                    TempData["ImagePath"] = listItems.ImagePath;

                    objs.Value = listItems.Value;

                    objs.ImagePath = listItems.ImagePath;

                    objLists.Add(objs);

                }

                return View(objLists);
            }
        }

        private ActionResult NotFound()
        {
            throw new NotImplementedException();
        }
        [HttpPost]
        public async Task<ActionResult> EditHomePageCounters(HomeClients model)
        {
            if (ModelState.IsValid)
            {
                string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";
                HomeClients obj = new HomeClients
                {
                    // Assuming HomePageCountersList is a property of HomeClients model
                    HomePageCountersList = model.HomePageCountersList,
                    FlagId = 2
                };


                using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
                {
                    // Ensure you're sending the correct object to the API
                    HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewAddHomePageCounter", obj);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = await responseMessage.Content.ReadAsStringAsync();
                        var response = JObject.Parse(responseData);
                        bool isStatus = Convert.ToBoolean(response.SelectToken("Status"));

                        if (isStatus)
                        {
                            TempData["Success"] = "Home Page Counters updated successfully.";
                            return RedirectToAction("HomePageCounters");
                        }
                        else
                        {
                            ModelState.AddModelError("", response.SelectToken("Message").ToString());
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while updating the data.");
                    }
                }
                List<HomeClients> objLists = new List<HomeClients>();
                foreach (var listItems in homePageCounter)
                {
                    HomeClients objs = new HomeClients();

                    //objs.TextEn = listItems.TextEn;
                    //TempData["TextEn"] = listItems.TextEn;
                    //TempData["value"] = listItems.Value;
                    //TempData["ImagePath"] = listItems.ImagePath;

                    //objs.Value = listItems.Value;

                    //objs.ImagePath = listItems.ImagePath;

                    objLists.Add(objs);

                }
                //  return View(model);
            }
            return View(model);

            // Return the view with the current model if validation fails or the HTTP request fails
            //  return View(model);
        }



        public async Task<ActionResult> DeleteHomePageCounters(int id)
        {
            string ses = Convert.ToString(Session["AdminUserId"]);
            HomeClients obj = new HomeClients();

            obj.FlagId = 3;
            obj.Id = id;
            string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";
            using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
            {
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageClientsAPI/NewAddHomePageClients", obj);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var Response = JObject.Parse(responseData);
                    bool isStatus = Convert.ToBoolean(Response.SelectToken("Status"));
                    string Message = Response.SelectToken("Message").ToString();

                    if (isStatus == true)
                    {
                        TempData["Success"] = "Deleted Home Page Clients Successful.";
                        obj.Create = true;
                    }
                    else
                    {
                        TempData["Error"] = "Transaction Timeout!.";
                    }
                }

                return RedirectToAction("HomePageClients");
            }
        }
            private byte[] StreamToBytes(Stream strm)
        {
            throw new NotImplementedException();
        }

        public class ApiResponse
        {
            public List<HomeClients> Data { get; set; }
            public bool Status { get; internal set; }
        }
    }
}

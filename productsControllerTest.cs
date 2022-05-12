using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using System;
using helloAPI.DTO;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace helloAPI.integration.test;

public class helloAPIIntegrationTest
{
    private readonly HttpClient _client = null!;
    private ApplicationDbContext applicationDbContext = null!;
    public helloAPIIntegrationTest(){
        

        //this.applicationDbContext.SaveChangesAsync();
        var application = new WebApplicationFactory<Program>().WithWebHostBuilder(
            builder => {
                builder.ConfigureServices(
                    services => {
                        // Remove the app's ApplicationDbContext registration.
    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof (DbContextOptions < ApplicationDbContext > ));

    if (descriptor != null) {
        services.Remove(descriptor);
    }

    var descriptor2 = services.SingleOrDefault(d => d.ServiceType == typeof (ApplicationDbContext));

    if (descriptor2 != null) {
        services.Remove(descriptor2);
    }

                        services.AddDbContext<ApplicationDbContext>(
                        options => { options.UseInMemoryDatabase(databaseName: "integrationTestInMemoryDb"); }
                         );

                         var sp = services.BuildServiceProvider();

                         var scope = sp.CreateScope();
                        
                        var scopedServices = scope.ServiceProvider;
                        applicationDbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

                        applicationDbContext.Database.EnsureCreated();

                        

                    }
                );
            }
        );
        _client = application.CreateClient();
    }

    [Fact]
    public async void Get_Returns_Products()
    {

        var response = await _client.GetAsync("/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        //Assert.True(true);
    }

    [Fact]
    public async void Get_Product_By_ID_Returns_Correct_Product(){

        //get first product
        var productFromDb = await applicationDbContext.Products.FirstOrDefaultAsync();

        var response = await _client.GetAsync("/products/" + productFromDb?.Guid );
        
        var json = await response.Content.ReadAsStringAsync();

        var productFromEndpoint = JsonConvert.DeserializeObject<ProductsDTO>(json);

        Console.WriteLine( "Product Name = " + productFromEndpoint?.Name );

        Assert.True(productFromEndpoint?.Name == productFromDb?.Name);
    }
}
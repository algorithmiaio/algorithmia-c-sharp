[![NuGet](https://img.shields.io/nuget/v/Algorithmia.Client.svg)](https://www.nuget.org/packages/Algorithmia.Client) [![Build status](https://dev.azure.com/algorithmia/Algorithmia%20.NET%20SDK/_apis/build/status/Algorithmia%20.NET%20SDK)](https://dev.azure.com/algorithmia/Algorithmia%20.NET%20SDK/_build/latest?definitionId=9) ![Deployment Status](https://vsrm.dev.azure.com/algorithmia/_apis/public/Release/badge/d1f7fd3a-6b4d-4c5c-8ff1-053d239ffc67/1/1)

# Algorithmia Client Library for .NET

Algorithmia is a hosted marketplace as well as an on-premises enterprise solution that allows developers to tap into a catalog of over 4,500 algorithmic microservices.  Developers leverage one of the existing algorithms, micrososervices, or functions or write their own.

### Adding Algorithmia Client to your .NET Project
The best and easiest way to add the Algorithmia Client to your .NET projects is to use NuGet package manager.  Within  Visual Studio, you can use the NuGet GUI to search for and install the Algorithmia Client NuGet package.  Or, as a shortcut, simply type the following command in the Package Manager Console:

    Install-Package Algorithmia.Client

### Sample Usage

To get started you will want to initialize an Algorithmia Client with your [API key](https://algorithmia.com/developers/basics/customizing-api-keys/).

    var client = new Algorithmia.Client("YOUR_API_KEY");

Next, you can specify an algorithm you would like to run and then process the algorithm's response.  In this case we are going to call the [Hello demo algorithm](https://algorithmia.com/algorithms/demo/hello).

     var algo = new Algorithmia.Algorithm(client, "algo://demo/hello");
     var response = algo.pipe<string>("World");

     Console.WriteLine(response.result);

## Getting help

If you need help installing or using the library, please contact Algorithmia Support at info@algorithmia.com or send us a message on https://algorithmia.com.

If you have found a bug in the library or would like new features added, go ahead and open issues or pull requests against this repo!

#### [Getting Started with Algorithmia][0]
#### [Algorithmia Recipes][1]
#### [Algorithmia Developer Center and Documentation][2]

[0]: https://algorithmia.com/developers/getting-started/
[1]: https://algorithmia.com/developers/tutorials/recipes
[2]: https://algorithmia.com/developers/

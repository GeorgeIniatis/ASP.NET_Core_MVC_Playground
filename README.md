## ASP.NET Core MVC Playground

This is just a playground site where multiple ASP.NET Core, Javascript and Bootstrap elements have been implemented as a way to get familiar with them.

### Running Site Locally

**Assuming you are using Visual Studio 2019 or Visual Studio 2022**

1. Inside `secrets.json` which can be accessed by right-clicking on your project and clicking `Manage User Secrets`
input the following information following the exact format and replacing the dictionary values with your own API Keys. You do not have to enter every single one but the ConnectionStrings are a must.

```
// File Format
{
  "ConnectionStrings": {
    "IdentityDb": "IdentityDb",
    "DataDb": "DataDb"
  },
  "Authentication": {
    "Google": {
      "ClientId": "ClientId",
      "ClientSecret": "ClientSecret"
    },
    "Facebook": {
      "AppID": "AppID",
      "AppSecret": "AppSecret"
    },
    "Twitter": {
      "ApiKey": "ApiKey",
      "SecretKey": "SecretKey",
    },
    "Microsoft": {
      "ClientID": "ClientID",
      "ClientSecret": "ClientSecret"
    }
  },
  "EmailService": {
    "Mailjet": {
      "ApiKey": "ApiKey",
      "SecretKey": "SecretKey"
    },
    "Sendgrid": {
      "ApiKey": "ApiKey"
    }
  },
  "SmsService": {
    "Twilio": {
      "AccountSID": "AccountSID",
      "AuthToken": "AuthToken",
      "PhoneNumber": "+PhoneNumber"
    }
  },
  "AdminAccount": {
    "FirstName": "Admin",
    "LastName": "McAdmin",
    "Email": "admin@site.com",
    "Password": "adminTestpa%$ssword3648"
  },
  "DbResourceConfiguration": {
    "ConnectionString": "ConnectionString",
    "GoogleApiKey": "GoogleApiKey",
  },
  "Stripe": {
    "PublishableKey": "PublishableKey",
    "SecretKey": "SecretKey",
    "EndpointSecretKey": "EndpointSecretKey"
  }
}
```

2. Data Migrations

```
add-migration initialCreate -context DataDbContext -o "Data/Migrations"
update-database -context DataDbContext
```

3. Identity Migrations

```
add-migration initialCreate -context IdentityDbContext - o "Areas/Identity/Data/Migrations"
update-database -context IdentityDbContext
```
4. Run XUnit Tests

5. Access Site

* **You can register a normal user and sign in but you will only have access to a limited number of pages or**

* **Login with admin user automatically created to access all the available pages**

```
Username: admin@site.com
Password: adminTestpa%$ssword3648
```

6. Buy Something

* **Using an account go to the Items page and add stuff to the basket. Then go to the Checkout page and enter one of the testing 'credit cards' which you can find [here](https://stripe.com/docs/checkout/quickstart#testing).**

7. Test Stripe Webhook Integration

* **Download Stripe CLI and run the following commands**

```
stripe login
stripe listen --forward-to https://localhost:44395/webhook
```

8. Disclaimers

* **Only the Items Page has been setup fully for Localisation and Globalization, but without the database table changing the locale will have almost no effect except showing the variable names used**

* **The SMS service will only work with number you have chosen during the set up of Sendgrid.**

* **The Email service may or may not work. What I would suggest is using a temp email.**

# JSON Generator

A server-rendered JSON Generator web app built with **.NET 9 Blazor Server** that allows users to define JSON schemas with embedded JavaScript-like syntax using Faker/Chance, and generates mock data instantly. The application also supports generating a **unique URL** for sharing or fetching the generated data.

<img width="1918" height="946" alt="Screenshot 2025-07-30 084454" src="https://github.com/user-attachments/assets/c6013cec-d133-45eb-a1c7-96d41b6a69ba" />

---

## ✨ Features

- ✅ **Input as JSON, Output as JSON**
- 🧠 Evaluate JavaScript-like functions using [Jint](https://github.com/sebastienros/jint)
- 🌍 Support for data mocking using [`@faker-js/faker`](https://www.npmjs.com/package/@faker-js/faker) and [`chance`](https://www.npmjs.com/package/chance)
- 🔁 Supports `repeat(n, m)` syntax to generate arrays of varying lengths
- 🧩 Automatically detects and executes embedded JavaScript in JSON
- 🔗 Share generated data via unique GET endpoint URLs
- 🌐 Built with **.NET 9 Blazor Server**

---

## 📥 Sample Input (JSON Schema)

```json
[
  "repeat(2,3)",
  {
    "message": "Hello, faker.person.firstName()! Your order number is: #faker.number.int()",
    "phoneNumber": "faker.phone.number()",
    "phoneVariation": "+90 faker.number.int({ min: 300, max: 399 }) faker.number.int()",
    "status": "faker.helpers.arrayElement(['active', 'disabled'])",
    "name": {
      "first": "faker.person.firstName()",
      "middle": "faker.person.middleName()",
      "last": "faker.person.lastName()"
    },
    "username": "this.name.first + '.' + this.name.last",
    "password": "faker.internet.password()",
    "emails": [
      "repeat(5,6)",
      "faker.internet.email(undefined, undefined, faker.helpers.arrayElement(['gmail.com','hotmail.com','yahoo.com']))"
    ],
    "location": {
      "street": "faker.location.streetAddress()",
      "city": "faker.location.city()",
      "state": "faker.location.state()",
      "country": "faker.location.country()",
      "zip": "faker.location.zipCode()",
      "coordinates": {
        "latitude": "faker.location.latitude()",
        "longitude": "faker.location.longitude()"
      }
    }
  }
]
```

---

## 📤 Output

Returns fully mocked JSON with evaluated data:

```json
[
  {
    "message": "Hello, John! Your order number is: #42",
    "phoneNumber": "323.572.6461 x1129",
    "phoneVariation": "+90 340 860 33 71",
    "status": "disabled",
    "name": {
      "first": "Mable",
      "middle": "Jamie",
      "last": "Haley"
    },
    "username": "Mable.Haley",
    "password": "j2STckZcf$B_ms",
    "emails": [
      "Felipa70@hotmail.com",
      "Gilbert92@hotmail.com"
    ],
    "location": {
      "street": "546 S Washington Blvd",
      "city": "New York",
      "state": "NY",
      "country": "USA",
      "zip": "10001",
      "coordinates": {
        "latitude": "40.7128",
        "longitude": "-74.0060"
      }
    }
  }
]
```

---

## 🔗 API Endpoint

Each generation creates a unique URL to access your data via HTTP GET:

```
https://jsondatagenerator.com/u/{unique-id}
```

---

## 📦 Packages Used

### NPM
- [`@faker-js/faker`](https://www.npmjs.com/package/@faker-js/faker) 
- [`chance`](https://www.npmjs.com/package/chance) 

### NuGet (.NET 9)
- `BlazorMonaco` 
- `Jint`

---

## 🧪 Getting Started (Local Dev)

```bash
git clone https://github.com/your-repo/json-generator
cd json-generator
dotnet build
dotnet run
```

Then visit `http://localhost:5000` in your browser.

---

## 📄 License

MIT License. See `LICENSE` file.

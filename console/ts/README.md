# Obfuscated TypeScript Code

## Overview

This project contains an obfuscated TypeScript file that outputs `"why"` to the console when executed. The script has been intentionally designed to be difficult to analyze by using a wide range of TypeScript constructs such as interfaces, classes, generics, union types, intersection types, enums, and more.

## How It Works

1. **Obfuscation Setup:**  
   - The code stores obfuscated configuration strings (e.g., `"base64"`, `"from"`, `"toString"`, `"binary"`) in an array using escape sequences.
   - It then encodes the original code (`console.log('why');`) into Base64 and reverses the string to further obscure its content.

2. **Utility Functions and Types:**  
   - The script defines several type aliases, interfaces, and generic types.
   - It uses functions such as a string reversal function and an identity function to complicate the logic.

3. **Decoding Process:**  
   - A generic decoder class attempts to decode the reversed Base64 string using the `atob` function for browsers.
   - If an error occurs (e.g., due to incompatible encoding in Node.js), the code falls back to using `Buffer.from` to decode the string.

4. **Code Execution:**  
   - The decoded string is executed using an executor class that calls `eval`, thereby printing `"why"` to the console.
   - Additional constructs, like a value holder and a wrapper, are used to further increase the complexity.

5. **Environment Detection:**  
   - The code uses an enum to determine whether it is running in a browser or in Node.js, logging the environment along with dummy data.

## Prerequisites

- **Node.js (v20+):** For running the script in a Node environment.
- **TypeScript Compiler (tsc):** To compile the TypeScript file into JavaScript.

## Installation

1. **Install Node.js:**  
   Download and install Node.js (v20 or higher) from [nodejs.org](https://nodejs.org/).

2. **Install TypeScript Compiler:**  
   If you haven’t installed it globally, run:
   ```bash
   npm install -g typescript
   ```

## Compilation and Execution

### Running with Node.js

1. **Compile the TypeScript File:**  
   Navigate to the project directory and compile the file:
   ```bash
   tsc why.ts
   ```
   This will generate a `why.js` file.

2. **Execute the JavaScript File:**  
   Run the compiled file using Node.js:
   ```bash
   node why.js
   ```
   You should see the output `"why"` along with additional log information indicating the environment and dummy values.

### Running in a Browser

1. **Compile the TypeScript File:**  
   As described above, compile the file using `tsc`.

2. **Include in an HTML File:**  
   Create an HTML file and include the generated JavaScript file:
   ```html
   <!DOCTYPE html>
   <html lang="en">
   <head>
     <meta charset="UTF-8">
     <meta name="viewport" content="width=device-width, initial-scale=1.0">
     <title>Obfuscated TS Demo</title>
   </head>
   <body>
     <script src="why.js"></script>
   </body>
   </html>
   ```

3. **Open the HTML File in a Browser:**  
   Open the HTML file in your browser and check the browser console to see the output `"why"`.

## How to Modify

- **Source Code:**  
  The source code is highly obfuscated. If you need to modify it, make sure to preserve the underlying logic and the decoding mechanism.

- **Obfuscation:**  
  The reversal of the Base64 string and its storage in an obfuscated format are key parts of the design. Any changes to this process might result in decoding errors.

## Disclaimer

This script is provided as an example of obfuscated TypeScript code. It is meant for educational purposes and should be used with caution in production environments due to potential maintainability and security concerns.

---

For any issues or questions, please open an issue in the repository.

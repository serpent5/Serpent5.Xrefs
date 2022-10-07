const defaultTheme = require("tailwindcss/defaultTheme");

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./index.html", "./src/**/*.{js,jsx}"],
  plugins: [require("@tailwindcss/forms")],
  theme: {
    extend: {
      fontFamily: {
        sans: [
          "Roboto",
          ...defaultTheme.fontFamily.sans.filter(x => x !== "Roboto")
        ]
      }
    }
  }
};

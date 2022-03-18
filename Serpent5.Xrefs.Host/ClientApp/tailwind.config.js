const defaultTheme = require("tailwindcss/defaultTheme");

module.exports = {
  content: ["./src/**/*.ejs", "./src/**/*.html", "./src/**/*.jsx", "./src/**/*.html"],
  plugins: [require("@tailwindcss/forms")],
  theme: {
    extend: {
      fontFamily: {
        sans: ["Roboto", ...defaultTheme.fontFamily.sans.filter(x => x !== "Roboto")]
      }
    }
  }
};

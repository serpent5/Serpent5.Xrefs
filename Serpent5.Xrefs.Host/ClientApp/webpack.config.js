const path = require("path");
const { merge: webpackMerge } = require("webpack-merge");

const CleanPlugin = require("clean-webpack-plugin").CleanWebpackPlugin;
const CopyPlugin = require("copy-webpack-plugin");
const CSSMinimizerPlugin = require("css-minimizer-webpack-plugin");
const ESLintPlugin = require("eslint-webpack-plugin");
const HTMLPlugin = require("html-webpack-plugin");
const MiniCSSExtractPlugin = require("mini-css-extract-plugin");
const StylelintPlugin = require("stylelint-webpack-plugin");

module.exports = function getWebpackConfig(_, { mode: webpackMode }) {
  const isDevelopment = webpackMode === "development";
  const filenameBase = isDevelopment ? "[name]" : "[name].[contenthash]";
  const sourceContext = path.resolve(__dirname, "src");

  return webpackMerge(
    getBaseWebpackConfig(isDevelopment, sourceContext, filenameBase),
    isDevelopment ? getDevelopmentWebpackConfig() : getProductionWebpackConfig(filenameBase)
  );
};

const jsExtensions = [".js", ".jsx"];

function getBaseWebpackConfig(isDevelopment, sourceContext, filenameBase) {
  return {
    context: __dirname,
    devServer: {
      client: {
        overlay: {
          errors: true,
          warnings: true
        }
      },
      hot: true,
      https: true,
      open: true,
      proxy: {
        "/api": {
          secure: false,
          target: "https://localhost:5001"
        }
      }
    },
    entry: {
      app: [
        "core-js/stable",
        "regenerator-runtime/runtime",
        path.resolve(sourceContext, "index.jsx"),
        path.resolve(sourceContext, "index.css")
      ]
    },
    module: {
      rules: [
        {
          test: /\.jsx?/,
          exclude: /node_modules/,
          use: {
            loader: "babel-loader",
            options: {
              presets: [
                ["@babel/preset-react", { runtime: "automatic" }],
                ["@babel/preset-env", { corejs: "3", useBuiltIns: "entry" }]
              ]
            }
          }
        },
        {
          test: /\.css$/,
          exclude: /node_modules/,
          use: [
            isDevelopment ? "style-loader" : MiniCSSExtractPlugin.loader,
            {
              loader: "css-loader",
              options: {
                importLoaders: 1
              }
            },
            {
              loader: "postcss-loader",
              options: {
                postcssOptions: {
                  plugins: ["tailwindcss", "postcss-preset-env"]
                }
              }
            }
          ]
        }
      ]
    },
    output: {
      hashFunction: "sha256",
      path: path.resolve(__dirname, "build/"),
      filename: `${filenameBase}.js`
    },
    plugins: [
      new CleanPlugin(),
      new CopyPlugin({
        patterns: [
          {
            from: path.resolve(__dirname, "public"),
            noErrorOnMissing: true
          }
        ]
      }),
      new HTMLPlugin({
        inject: false,
        meta: false,
        title: "Serpent5.Xrefs"
      }),
      new StylelintPlugin({
        context: sourceContext
      }),
      new ESLintPlugin({
        context: sourceContext,
        extensions: jsExtensions
      })
    ],
    resolve: {
      extensions: jsExtensions
    }
  };
}

function getDevelopmentWebpackConfig() {
  return {
    devtool: "eval-source-map"
  };
}

function getProductionWebpackConfig(filenameBase) {
  return {
    bail: true,
    optimization: {
      minimizer: ["...", new CSSMinimizerPlugin()],
      runtimeChunk: "single",
      splitChunks: {
        chunks: "all",
        name: "vendor"
      }
    },
    plugins: [
      new MiniCSSExtractPlugin({
        filename: `${filenameBase}.css`
      })
    ]
  };
}

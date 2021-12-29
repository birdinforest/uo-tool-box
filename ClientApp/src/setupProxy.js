const createProxyMiddleware = require('http-proxy-middleware');
const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:47624';

const context =  [
  "/weatherforecast",
  "/journey",
];

module.exports = function(app) {
  console.log('env.ASPNETCORE_HTTPS_PORT', env.ASPNETCORE_HTTPS_PORT);
  console.log('env.ASPNETCORE_URLS', env.ASPNETCORE_URLS);
  
  const appProxy = createProxyMiddleware(context, {
    target: target,
    secure: false
  });

  app.use(appProxy);
};

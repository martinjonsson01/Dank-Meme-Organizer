const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp();
const express = require('express');
const app = express();
// Imports the Google Cloud client library
const vision = require('@google-cloud/vision');
// Creates a client
const client = new vision.ImageAnnotatorClient();


// Express middleware that validates Firebase ID Tokens passed in the Authorization HTTP header.
// The Firebase ID token needs to be passed as a Bearer token in the Authorization HTTP header like this:
// `Authorization: Bearer <Firebase ID Token>`.
// when decoded successfully, the ID Token content will be added as `req.user`.
const authenticate = (req, res, next) => {
    if (!req.headers.authorization || !req.headers.authorization.startsWith('Bearer ')) {
        res.status(403).send('Unauthorized');
        return;
    }
    const idToken = req.headers.authorization.split('Bearer ')[1];
    admin.auth().verifyIdToken(idToken).then((decodedIdToken) => {
        req.user = decodedIdToken;
        return next();
    }).catch(() => {
        res.status(403).send('Unauthorized');
    });
};

app.use(authenticate);

app.post('/analyzeimage', (request, response) => {

    const imageBase64 = request.body;
    const features = [
        { type: "DOCUMENT_TEXT_DETECTION", maxResults: 1 },
        { type: "IMAGE_PROPERTIES", maxResults: 1 },
        { type: "SAFE_SEARCH_DETECTION", maxResults: 1 },
        { type: "WEB_DETECTION", maxResults: 5 }
    ];
    const visionRequest = {
        image: { content: imageBase64 },
        features: features
    };

    console.time("Vision request");

    client
        .annotateImage(visionRequest)
        .then(result => {
            response.status(200).send(result);
            console.timeEnd("Vision request");
            return result;
        })
        .catch(err => {
            console.log(err);
            response.status(500).send(err);
            console.timeEnd("Vision request");
        });
});

// Expose the API as a function
exports.api = functions.https.onRequest(app);

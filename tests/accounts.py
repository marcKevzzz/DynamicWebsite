"""Exercise account forms against a running local app backed by a disposable database.

Run: python tests/accounts.py http://localhost:5080
Creates one regular account and its audit entries per run.
"""
import sys
from html.parser import HTMLParser
from http.cookiejar import CookieJar
from urllib.error import HTTPError
from urllib.parse import urlencode, urljoin, urlsplit
from urllib.request import build_opener, HTTPCookieProcessor, Request
from uuid import uuid4


class Fields(HTMLParser):
    def __init__(self, html):
        super().__init__()
        self.values = {}
        self.feed(html)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == "input" and "name" in attrs:
            self.values[attrs["name"]] = attrs.get("value", "")


def session(origin):
    assert urlsplit(origin).hostname in {"localhost", "127.0.0.1", "::1"}, "Use a local test database"
    browser = build_opener(HTTPCookieProcessor(CookieJar()))

    def request(path, data=None, *, content_type=None, binary=False):
        payload = urlencode(data).encode() if data is not None and not isinstance(data, bytes) else data
        headers = {"Content-Type": content_type} if content_type else {}
        try:
            result = browser.open(Request(urljoin(origin, path), data=payload, headers=headers))
        except HTTPError as error:
            result = error
        with result:
            body = result.read()
            return result.status, urlsplit(result.url).path, body if binary else body.decode()

    def form(path, data, token_page=None):
        html = request(token_page or path)[2]
        token = Fields(html).values["__RequestVerificationToken"]
        return request(path, {**data, "__RequestVerificationToken": token})

    return request, form


def run(origin):
    request, form = session(origin)
    assert request("/")[1] == "/account/sign-in"
    assert request("/account/profile")[1] == "/account/sign-in"
    email = f"account-{uuid4().hex}@example.test"
    password = uuid4().hex + "A!"
    details = dict(FirstName="Account", LastName="Check", Email=email)
    registration = {**details, "Password": password, "ConfirmPassword": password}
    assert form("/account/sign-up", registration)[1] == "/account/sign-in"
    assert form("/account/sign-up", {**registration, "Email": email.upper()})[1] == "/account/sign-up"
    assert request("/account/sign-in", dict(Email=email, Password=password))[0] == 400
    assert form("/account/sign-in", dict(Email=email, Password=password))[1] == "/"
    assert request("/admin")[0] == 403
    changed = {**details, "FirstName": "Updated"}
    page = form("/account/profile", changed)[2]
    assert "Your profile has been saved." in page
    updated_email = "new-" + email
    changed["Email"] = updated_email
    status = form("/account/change-email", changed, "/account/profile")[0]
    assert status in (404, 405), status
    assert form("/account/confirm-email", {"Email": updated_email, "Token": "old"}, "/account/profile")[0] == 400
    page = form("/account/profile", changed)[2]
    assert email in page and updated_email not in page and "Change email" not in page
    assert Fields(request("/account/profile")[2]).values["FirstName"] == "Updated"
    assert form("/account/logout", {}, "/account/profile")[1] == "/account/sign-in"
    assert request("/")[1] == "/account/sign-in"
    assert form("/account/sign-in", dict(Email=updated_email, Password=password))[1] == "/account/sign-in"
    assert form("/account/sign-in", dict(Email=email, Password=password))[1] == "/"
    print("Account registration, authentication, authorization and profile checks passed.")


if __name__ == "__main__":
    run(sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5080")

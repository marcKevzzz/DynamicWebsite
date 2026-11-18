"""Run against a disposable local DB. Requires the same administrator variables as reports.py."""
import base64
import os
import re
import sys
from uuid import uuid4

from accounts import Fields, session


def run(origin):
    request, form = session(origin)
    assert form("/account/sign-in", dict(Email=os.environ["DCD_TEST_ADMIN_EMAIL"],
                Password=os.environ["DCD_TEST_ADMIN_PASSWORD"]))[1].startswith("/admin")
    name = "CMS " + uuid4().hex
    values = {"Fields[Project name]": name, "Fields[Category]": "Category", "Fields[Description]": "Description",
              "Fields[Technologies]": "HTML, CSS", "Fields[Website]": "https://example.test/"}
    page = "/admin/works/new"
    post = "/admin/content/works/new"
    html = request(page)[2]
    fields = Fields(html).values
    image = base64.b64decode("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aMwAAAABJRU5ErkJggg==")
    boundary = uuid4().hex
    parts = [f'--{boundary}\r\nContent-Disposition: form-data; name="{k}"\r\n\r\n{v}\r\n'.encode()
             for k, v in {**values, "__RequestVerificationToken": fields["__RequestVerificationToken"]}.items()]
    parts += [f'--{boundary}\r\nContent-Disposition: form-data; name="Uploads"; filename="art.png"\r\nContent-Type: image/png\r\n\r\n'.encode(),
              image, f"\r\n--{boundary}--\r\n".encode()]
    status, page, html = request(post, b"".join(parts), content_type="multipart/form-data; boundary=" + boundary)
    assert status == 200 and page.startswith("/admin/works/") and not page.endswith("/new"), (status, page)
    post = page.replace("/admin/", "/admin/content/")
    fields = Fields(html).values
    revision = fields["Revision"]
    image_path = re.search(r'data-image-path="(uploads/[^"]+)"', html)[1]
    assert request("/images/" + image_path, binary=True)[2] == image
    assert name in request("/")[2]
    values.update({"Images": image_path, "Revision": revision})
    invalid = {**values, "Fields[Website]": "not a website"}
    assert "Use an http or https URL" in form(post, invalid, page)[2]
    updated = name + " saved"
    assert updated in form(post, {**values, "Fields[Project name]": updated}, page)[2]
    assert "Another edit changed this record" in form(post, values, page)[2]
    assert updated in request("/")[2]
    latest = Fields(request(page)[2]).values["Revision"]
    assert form(post + "/delete", {"Revision": latest}, page)[1] == "/admin/works"
    assert updated not in request("/")[2] and request("/images/" + image_path)[0] == 404
    service = {"Fields[Title]": name, "Fields[Description]": "Check ordered tags",
               "Fields[Focus]": "REACT, Custom tag"}
    status, page, html = form("/admin/content/services/new", service, "/admin/services/new")
    assert status == 200 and page.startswith("/admin/services/") and not page.endswith("/new")
    post = page.replace("/admin/", "/admin/content/")
    service["Fields[Focus]"] = "Custom tag, REACT"
    _, _, html = form(post, {**service, "Revision": Fields(html).values["Revision"]}, page)
    assert Fields(html).values["Fields[Focus]"] == "Custom tag, REACT"
    public = request("/")[2].split(name, 1)[1].split("</ul>", 1)[0]
    assert public.index(">Custom tag<") < public.index(">REACT<")
    assert public.count("<svg") == 2
    assert form(post + "/delete", {"Revision": Fields(html).values["Revision"]}, page)[1] == "/admin/services"
    print("Project CRUD, uploads, validation, stale edits and ordered service tag rendering passed.")


if __name__ == "__main__":
    run(sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5080")

from playwright.sync_api import sync_playwright
import os

def run(playwright):
    browser = playwright.chromium.launch(headless=True)
    page = browser.new_page()

    # Load the local HTML file
    file_path = os.path.abspath("verification/mock_error_page.html")
    page.goto(f"file://{file_path}")

    # Take screenshot
    page.screenshot(path="verification/error_page_mock.png")

    browser.close()

with sync_playwright() as playwright:
    run(playwright)
